using System.Globalization;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Domain;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

/// <summary>Projection کامل سفارش (بدون تکرار کد — DRY).</summary>
internal static class OrderQueryExtensions
{
    public static IQueryable<OrderDetailDto> SelectFullOrder(this IQueryable<Domain.Order> query) =>
        query.Select(o => new OrderDetailDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            StatusTitle = OrderTitles.StatusTitle(o.Status),
            StatusBadgeClass = OrderTitles.StatusBadgeClass(o.Status),
            PaymentMethodTitle = o.PaymentMethod == PaymentMethod.Online ? "پرداخت اینترنتی" : "پرداخت در محل",
            ShippingMethodTitle = o.ShippingMethod == ShippingMethod.Express ? "ارسال اکسپرس" : "ارسال با پست",
            Subtotal = o.Subtotal,
            DiscountAmount = o.DiscountAmount,
            ShippingCost = o.ShippingCost,
            TaxAmount = o.TaxAmount,
            Total = o.Total,
            DiscountCode = o.DiscountCode,
            ReceiverName = o.ReceiverName,
            Phone = o.Phone,
            StateName = o.State!.Name,
            CityName = o.City!.Name,
            Address = o.Address,
            PostalCode = o.PostalCode,
            Note = o.Note,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Slug = i.Product.Slug,
                Image = i.ProductImage,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                Quantity = i.Quantity,
                LineTotal = i.UnitPrice * i.Quantity,
                VariantText = i.VariantText,
                SellerName = i.SellerName ?? "مارکت"
            }).ToList()
        });
}

/// <summary>
/// چرخه حیات سفارش — همه عملیات حساس داخل تراکنش اتمیک.
/// </summary>
public class OrderService(
    ApplicationDbContext db,
    ISettingsService settings,
    ICartService cartService,
    IInventoryService inventory,
    Application.Interfaces.ILoyaltyService loyalty,
    Application.Interfaces.IEventPublisher events,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<PlaceOrderResult> PlaceOrderAsync(Guid userId, CheckoutViewModel model, CancellationToken ct = default)
    {
        var config = await settings.GetAsync(ct);

        // رزرو فعال کاربر باید معتبر باشد (با قیمت از منبع: آفر / تنوع / مارکت)
        var now = DateTime.UtcNow;
        var reservations = await db.StockReservations
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.Status == ReservationStatus.Active && r.ExpiresAt > now)
            .Select(r => new
            {
                r.Id,
                r.ProductId,
                r.OfferId,
                r.VariantId,
                r.Quantity,
                r.Product.Name,
                r.Product.MainImage,
                r.Product.DiscountPercent,
                SellerId = r.Offer != null ? (long?)r.Offer.SellerId : null,
                OfferPrice = r.Offer != null ? (long?)r.Offer.Price : null,
                OfferDiscount = r.Offer != null ? (byte?)r.Offer.DiscountPercent : null,
                VariantAttributesJson = r.Variant != null ? r.Variant.AttributesJson : null,
                UnitPrice = r.Offer != null
                    ? (r.Offer.DiscountPercent > 0 ? r.Offer.Price - r.Offer.Price * r.Offer.DiscountPercent / 100 : r.Offer.Price)
                    : r.Variant != null ? r.Variant.FinalPrice
                    : (r.Product.DiscountPercent > 0 ? r.Product.UnitPrice - r.Product.UnitPrice * r.Product.DiscountPercent / 100 : r.Product.UnitPrice)
            })
            .ToListAsync(ct);

        if (reservations.Count == 0)
            return new PlaceOrderResult { Success = false, Message = "رزرو شما منقضی شده است. لطفاً دوباره تلاش کنید." };

        var subtotal = reservations.Sum(r => r.UnitPrice * r.Quantity);

        // گروه‌بندی مرسوله‌ها: کالاهای مارکت + هر فروشنده یک مرسوله مستقل (تعرفه/سقف اختصاصی از /seller/settings)
        var sellerIds = reservations.Where(r => r.SellerId != null).Select(r => r.SellerId!.Value).Distinct().ToList();
        var sellerCfg = await db.Sellers.AsNoTracking()
            .Where(s => sellerIds.Contains(s.Id))
            .Select(s => new { s.Id, s.ShippingCostOverride, s.FreeShippingThresholdOverride, s.PaymentMethods })
            .ToDictionaryAsync(x => x.Id, ct);

        var groups = reservations
            .GroupBy(r => r.SellerId)
            .Select(g => new ShipmentGroup(g.Sum(x => x.UnitPrice * x.Quantity),
                g.Key is null ? null : sellerCfg[g.Key.Value].ShippingCostOverride,
                g.Key is null ? null : sellerCfg[g.Key.Value].FreeShippingThresholdOverride))
            .ToList();

        // کد تخفیف (در سشن/فرم ارسال می‌شود)
        long discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(model.DiscountCode))
        {
            var validation = await discountValidation(model.DiscountCode, subtotal, ct);
            if (!validation.Success)
                return new PlaceOrderResult { Success = false, Message = validation.Message };
            discountAmount = validation.DiscountAmount;
        }

        var shippingMethod = (ShippingMethod)model.ShippingMethod;
        long shippingCost;
        if (shippingMethod == ShippingMethod.Express)
        {
            // اکسپرس: تعرفه مارکت × تعداد مرسوله‌ها (هر فروشنده مرسوله مستقل)
            shippingCost = config.ShippingExpressCost * Math.Max(1, groups.Count);
        }
        else
        {
            shippingCost = groups.Sum(g => MarketplaceShipping.PostCost(
                g.Subtotal, g.CostOverride, g.FreeOverride, config.ShippingPostCost, config.FreeShippingThreshold));
        }

        // مالیات بر ارزش افزوده (درصد قابل تنظیم از پنل ادمین)
        var taxAmount = subtotal * config.TaxPercent / 100;
        var total = Math.Max(0, subtotal - discountAmount) + shippingCost + taxAmount;
        var paymentMethod = (PaymentMethod)model.PaymentMethod;

        // روش پرداخت باید از سوی همه فروشندگانِ سبد پذیرفته شده باشد (فیلد PaymentMethods از /seller/settings)
        var pmCode = paymentMethod == PaymentMethod.Online ? "online" : "cod";
        foreach (var sid in sellerIds)
        {
            var accepted = (sellerCfg[sid].PaymentMethods ?? "online").Split(',');
            if (!accepted.Contains(pmCode))
                return new PlaceOrderResult
                {
                    Success = false,
                    Message = "یکی از فروشندگانِ سبد، روش پرداخت انتخابی را نمی‌پذیرد. لطفاً روش دیگری انتخاب کنید."
                };
        }

        return await Transactional.RunAsync(db, async token =>
        {
            var order = new Domain.Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                Status = paymentMethod == PaymentMethod.Online ? OrderStatus.AwaitingPayment : OrderStatus.Processing,
                PaymentMethod = paymentMethod,
                ShippingMethod = shippingMethod,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                ShippingCost = shippingCost,
                TaxAmount = taxAmount,
                Total = total,
                DiscountCode = discountAmount > 0 ? model.DiscountCode?.Trim().ToUpperInvariant() : null,
                ReceiverName = model.ReceiverName.Trim(),
                Phone = model.Phone.Trim(),
                StateId = model.StateId,
                CityId = model.CityId,
                Address = model.Address.Trim(),
                PostalCode = model.PostalCode,
                Note = model.Note,
                CreatedAt = DateTime.UtcNow
            };

            order.Items = reservations.Select(r => new Domain.OrderItem
            {
                ProductId = r.ProductId,
                ProductName = r.Name,
                ProductImage = r.MainImage,
                UnitPrice = r.UnitPrice,
                DiscountPercent = r.OfferDiscount ?? r.DiscountPercent,
                Quantity = r.Quantity,
                OfferId = r.OfferId,
                SellerId = r.SellerId,
                VariantId = r.VariantId,
                VariantText = ParseVariantText(r.VariantAttributesJson)
            }).ToList();

            // اسنپ‌شات نام فروشندگان
            var sellerIds = order.Items.Where(i => i.SellerId is not null).Select(i => i.SellerId!.Value).Distinct().ToList();
            Dictionary<long, (string StoreName, Guid OwnerUserId)> sellers = new();
            if (sellerIds.Count > 0)
            {
                var list = await db.Sellers
                    .Where(s => sellerIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.StoreName, s.OwnerUserId })
                    .ToListAsync(token);
                sellers = list.ToDictionary(s => s.Id, s => (s.StoreName, s.OwnerUserId));

                foreach (var item in order.Items.Where(i => i.SellerId is not null))
                    item.SellerName = sellers.GetValueOrDefault(item.SellerId!.Value).StoreName ?? "فروشنده مارکت‌پلیس";
            }

            db.Orders.Add(order);
            await db.SaveChangesAsync(token);

            // اتصال رزروهای فعال کاربر به سفارش
            await db.StockReservations
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.Active)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.OrderId, order.Id), token);

            // برای پرداخت آنلاین، تراکنش Pending ایجاد می‌شود
            if (paymentMethod == PaymentMethod.Online)
            {
                db.Payments.Add(new Domain.Payment
                {
                    OrderId = order.Id,
                    Amount = total,
                    Gateway = config.PaymentGateway,
                    Authority = Guid.NewGuid().ToString("N"),
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(token);
            }

            logger.LogInformation("سفارش {OrderNumber} برای کاربر {UserId} ثبت شد. مبلغ: {Total}",
                order.OrderNumber, userId, total);

            // اعلان‌ها (از کافکا منتشر می‌شود): خریدار + فروشندگان صاحب آفر
            await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                userId,
                $"سفارش {order.OrderNumber} ثبت شد",
                $"مبلغ: {total:N0} تومان — در انتظار پرداخت/پردازش",
                "bi-receipt", "/orders", Sms: true), token);

            foreach (var sid in sellerIds)
            {
                var s = sellers.GetValueOrDefault(sid);
                if (s.OwnerUserId == default || s.OwnerUserId == userId) continue;
                var count = order.Items.Count(i => i.SellerId == sid);
                await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                    s.OwnerUserId,
                    "🛒 فروش جدید!",
                    $"{count} قلم از آفرهای شما در سفارش {order.OrderNumber} قرار گرفت",
                    "bi-cart-check", "/seller"), token);
            }

            return new PlaceOrderResult
            {
                Success = true,
                Message = "سفارش با موفقیت ثبت شد.",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Total = total
            };
        }, ct);
    }

    public async Task<bool> MarkPaidAsync(int orderId, string gatewayRefId, CancellationToken ct = default)
    {
        var success = await Transactional.RunAsync(db, async token =>
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, token);
            if (order is null || order.Status != OrderStatus.AwaitingPayment) return false;

            // مصرف اتمیک رزروها: کسر از stock + reserved_count
            var consumed = await StockSql.ConsumeOrderAsync(db, orderId, token);
            if (!consumed) return false;

            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;

            // کارت انبار: خروج فروش
            foreach (var item in order.Items)
            {
                var stockAfter = await db.Products
                    .Where(p => p.Id == item.ProductId)
                    .Select(p => (int?)p.Stock)
                    .FirstOrDefaultAsync(token) ?? 0;

                db.StockMovements.Add(new Domain.StockMovement
                {
                    ProductId = item.ProductId,
                    Type = Domain.StockMovementType.Sale,
                    Delta = -item.Quantity,
                    StockAfter = stockAfter,
                    Reference = order.OrderNumber,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var payment = await db.Payments
                .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync(token);

            if (payment is not null)
            {
                payment.Status = PaymentStatus.Success;
                payment.RefId = gatewayRefId;
                payment.CompletedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(order.DiscountCode))
            {
                await db.Discounts
                    .Where(d => d.Code == order.DiscountCode)
                    .ExecuteUpdateAsync(s => s.SetProperty(d => d.UsedCount, d => d.UsedCount + 1), token);
            }

            await db.SaveChangesAsync(token);
            return true;
        }, ct);

        if (success)
        {
            var buyerId = await db.Orders
                .Where(o => o.Id == orderId)
                .Select(o => o.UserId)
                .FirstAsync(ct);

            // اعلان: پرداخت تأیید شد
            var paidOrderNumber = await db.Orders.Where(o => o.Id == orderId).Select(o => o.OrderNumber).FirstAsync(ct);
            await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                buyerId,
                $"✓ پرداخت سفارش {paidOrderNumber} تأیید شد",
                "کالای شما به‌زودی آماده‌سازی و ارسال می‌شود",
                "bi-check2-circle", "/orders", Sms: true), ct);

            // باشگاه مشتریان: امتیاز خرید + پاداش معرفی
            var orderTotal = await db.Orders.Where(o => o.Id == orderId).Select(o => o.Total).FirstAsync(ct);
            await loyalty.AddPointsAsync(buyerId, orderTotal, ct);
            await loyalty.PayReferralRewardAsync(buyerId, orderId, 500_000, ct);

            await cartService.ClearByUserAsync(buyerId, ct);
        }

        return success;
    }

    public async Task MarkPaymentFailedAsync(long paymentId, CancellationToken ct = default)
    {
        await db.Payments
            .Where(p => p.Id == paymentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PaymentStatus.Failed)
                .SetProperty(p => p.CompletedAt, DateTime.UtcNow), ct);
    }

    public async Task<PagedResult<OrderListItemDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 5, 50);
        page = Math.Max(1, page);

        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                StatusTitle = OrderTitles.StatusTitle(o.Status),
                StatusBadgeClass = OrderTitles.StatusBadgeClass(o.Status),
                Total = o.Total,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Sum(i => i.Quantity),
                FirstItemImage = o.Items.First().ProductImage
            })
            .ToListAsync(ct);

        return new PagedResult<OrderListItemDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<OrderDetailDto?> GetOrderAsync(int orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .SelectFullOrder()
            .FirstOrDefaultAsync(ct);

        return order;
    }

    public async Task<OrderDetailDto?> GetOrderForAdminAsync(int orderId, CancellationToken ct = default)
    {
        return await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .SelectFullOrder()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CancelAsync(int orderId, Guid userId, string reason, CancellationToken ct = default)
    {
        return await Transactional.RunAsync(db, async token =>
        {
            var order = await db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, token);

            if (order is null) return false;
            if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Canceled)
                return false;

            // بازگشت موجودی مصرف‌شده و آزادسازی رزروهای فعال
            await StockSql.RestockOrderAsync(db, orderId, token);

            // کارت انبار: برگشت از فروش
            foreach (var item in order.Items)
            {
                var stockAfter = await db.Products
                    .Where(p => p.Id == item.ProductId)
                    .Select(p => (int?)p.Stock)
                    .FirstOrDefaultAsync(token) ?? 0;

                db.StockMovements.Add(new Domain.StockMovement
                {
                    ProductId = item.ProductId,
                    Type = Domain.StockMovementType.Return,
                    Delta = item.Quantity,
                    StockAfter = stockAfter,
                    Reference = order.OrderNumber,
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Status = OrderStatus.Canceled;
            order.CancelReason = reason;
            await db.SaveChangesAsync(token);
            return true;
        }, ct);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken ct = default)
    {
        return await Transactional.RunAsync(db, async token =>
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, token);
            if (order is null) return false;

            if (status == OrderStatus.Canceled && order.Status != OrderStatus.Canceled)
            {
                await StockSql.RestockOrderAsync(db, orderId, token);
            }

            order.Status = status;
            if (status == OrderStatus.Shipped) order.ShippedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(token);

            // اعلان تغییر وضعیت به خریدار
            var statusTitle = status switch
            {
                OrderStatus.Paid => "پرداخت شد ✓",
                OrderStatus.Processing => "در حال پردازش",
                OrderStatus.Shipped => "ارسال شد 📦",
                OrderStatus.Delivered => "تحویل شد",
                OrderStatus.Canceled => "لغو شد",
                _ => status.ToString()
            };
            await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                order.UserId,
                $"سفارش {order.OrderNumber} {statusTitle}",
                null, "bi-box-seam", "/orders", Sms: status is OrderStatus.Shipped or OrderStatus.Canceled), token);

            return true;
        }, ct);
    }

    public async Task<PagedResult<OrderListItemDto>> GetFilteredForAdminAsync(string? status, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 10, 100);
        page = Math.Max(1, page);

        var query = db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var st))
            query = query.Where(o => o.Status == st);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(o => o.OrderNumber.Contains(q) || o.ReceiverName.Contains(q) || o.Phone.Contains(q));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                StatusTitle = OrderTitles.StatusTitle(o.Status),
                StatusBadgeClass = OrderTitles.StatusBadgeClass(o.Status),
                Total = o.Total,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Sum(i => i.Quantity),
                FirstItemImage = o.Items.First().ProductImage
            })
            .ToListAsync(ct);

        return new PagedResult<OrderListItemDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-6);

        var ordersPerDay = await db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= weekAgo)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count(), Amount = g.Sum(o => o.Total) })
            .ToListAsync(ct);

        var config = await settings.GetAsync(ct);

        var recentOrders = await db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.Id)
            .Take(6)
            .Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                StatusTitle = OrderTitles.StatusTitle(o.Status),
                StatusBadgeClass = OrderTitles.StatusBadgeClass(o.Status),
                Total = o.Total,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Sum(i => i.Quantity),
                FirstItemImage = o.Items.First().ProductImage
            })
            .ToListAsync(ct);

        var lowStock = await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Stock - p.ReservedCount <= config.LowStockThreshold)
            .OrderBy(p => p.Stock)
            .Take(6)
            .Select(p => new LowStockDto { ProductId = p.Id, Name = p.Name, Stock = p.Stock - p.ReservedCount })
            .ToListAsync(ct);

        var dashboard = new AdminDashboardDto
        {
            TotalOrders = await db.Orders.CountAsync(ct),
            TodayOrders = await db.Orders.CountAsync(o => o.CreatedAt >= today, ct),
            TotalSales = await db.Orders
                .Where(o => o.Status != OrderStatus.AwaitingPayment && o.Status != OrderStatus.Canceled)
                .SumAsync(o => (long?)o.Total, ct) ?? 0,
            TotalUsers = await db.Users.CountAsync(ct),
            TotalProducts = await db.Products.CountAsync(p => p.IsActive, ct),
            PendingComments = await db.Comments.CountAsync(c => !c.IsApproved, ct),
            UnreadMessages = await db.SiteMessages.CountAsync(m => !m.IsRead, ct),
            RecentOrders = recentOrders,
            LowStockProducts = lowStock
        };

        for (var i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var point = ordersPerDay.FirstOrDefault(o => o.Date == date);
            dashboard.OrdersPerDay.Add(new DailyPointDto
            {
                Label = Shop.Shared.PersianDateHelper.ToPersianLongDate(date),
                Value = point?.Count ?? 0,
                Amount = point?.Amount ?? 0
            });
        }

        return dashboard;
    }

    // ---- private ----

    private async Task<DiscountValidationResult> discountValidation(string code, long subtotal, CancellationToken ct)
    {
        code = code.Trim().ToUpperInvariant();
        var discount = await db.Discounts
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Code == code, ct);

        if (discount is null || !discount.IsActive)
            return new DiscountValidationResult { Success = false, Message = "کد تخفیف نامعتبر است." };

        if (discount.ExpireAt is not null && discount.ExpireAt < DateTime.UtcNow)
            return new DiscountValidationResult { Success = false, Message = "کد تخفیف منقضی شده است." };

        if (discount.UsageLimit is not null && discount.UsedCount >= discount.UsageLimit)
            return new DiscountValidationResult { Success = false, Message = "ظرفیت استفاده از کد تخفیف تکمیل شده است." };

        if (subtotal < discount.MinOrderAmount)
            return new DiscountValidationResult
            {
                Success = false,
                Message = $"حداقل مبلغ سفارش برای این کد {discount.MinOrderAmount.ToToman()} است."
            };

        var amount = subtotal * discount.Percent / 100;
        if (discount.MaxAmount is not null)
            amount = Math.Min(amount, discount.MaxAmount.Value);

        return new DiscountValidationResult
        {
            Success = true,
            Message = "کد تخفیف اعمال شد.",
            Code = code,
            DiscountAmount = amount
        };
    }

    private static string GenerateOrderNumber()
    {
        var stamp = DateTime.UtcNow.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
        var rnd = Random.Shared.Next(1000, 9999);
        return $"SH-{stamp}-{rnd}";
    }

    /// <summary>«آبی / XL» از JSON ویژگی‌های تنوع.</summary>
    private static string? ParseVariantText(string? attributesJson)
    {
        if (string.IsNullOrWhiteSpace(attributesJson)) return null;
        try
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Domain.ProductVariant.VariantAttribute>>(attributesJson);
            return items is { Count: > 0 } ? string.Join(" / ", items.Select(a => a.Value)) : null;
        }
        catch
        {
            return null;
        }
    }
}
