using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Messaging;

/// <summary>نگهدارندهٔ singleton تولیدکنندهٔ Kafka — فقط اگر BootstrapServers تنظیم شده باشد فعال است.</summary>
public class KafkaProducerHolder : IDisposable
{
    private readonly Lazy<IProducer<string, string>?> producer;

    public bool Enabled => producer.Value is not null;
    public string BootstrapServers { get; }
    public string Topic { get; }

    public KafkaProducerHolder(IConfiguration config)
    {
        BootstrapServers = config.GetValue<string>("Kafka:BootstrapServers", "") ?? "";
        Topic = config.GetValue<string>("Kafka:Topic", "shop.notifications") ?? "shop.notifications";
        producer = new Lazy<IProducer<string, string>?>(() =>
        {
            if (string.IsNullOrWhiteSpace(BootstrapServers)) return null;
            try
            {
                return new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = BootstrapServers }).Build();
            }
            catch
            {
                return null; // بروکر در دسترس نیست → fallback دیتابیس
            }
        });
    }

    /// <summary>انتشار غیرمسدودکننده — true یعنی به کافکا سپرده شد.</summary>
    public bool TryProduce(NotificationEvent evt)
    {
        var p = producer.Value;
        if (p is null) return false;
        try
        {
            p.Produce(Topic, new Message<string, string>
            {
                Key = evt.UserId.ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(evt)
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (producer.IsValueCreated)
        {
            try { producer.Value?.Flush(TimeSpan.FromSeconds(2)); } catch { }
            producer.Value?.Dispose();
        }
    }
}

/// <summary>
/// Publisher برنامه (Outbox Pattern): رویداد را در همان DbContext — و اگر فراخوان در تراکنش باشد،
/// در همان تراکنش دیتابیس — در جدول outbox ذخیره می‌کند. انتشار به کافکا کار OutboxDispatcherJob است؛
/// بنابراین نه درخواست بلاک می‌شود و نه رویدادی با rollback یا قطعی بروکر گم می‌شود.
/// </summary>
public class EventPublisher(ApplicationDbContext db, ILogger<EventPublisher> logger) : IEventPublisher
{
    public async Task PublishNotificationAsync(NotificationEvent evt, CancellationToken ct = default)
    {
        try
        {
            db.OutboxMessages.Add(new OutboxMessage
            {
                Type = "notification",
                Payload = System.Text.Json.JsonSerializer.Serialize(evt),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "نوشتن در outbox ناموفق بود — رویداد اعلان برای {User} از دست رفت", evt.UserId);
        }
    }
}

/// <summary>
/// OutboxDispatcherJob: هر ۵ ثانیه رویدادهای منتشرنشده را می‌خواند و به کافکا می‌فرستد
/// (بدون کافکا: مستقیم Notification می‌سازد). خطای بروکر → RetryCount++ و تلاش دوره‌ای؛
/// ردیف‌های منتشرشدهٔ قدیمی (بیش از ۷ روز) پاک می‌شوند.
/// </summary>
public class OutboxDispatcherJob(
    KafkaProducerHolder kafka,
    IServiceScopeFactory scopes,
    INotificationBroadcaster broadcaster,
    ILogger<OutboxDispatcherJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stopping)
    {
        logger.LogInformation("OutboxDispatcherJob شروع به کار کرد (کافکا: {Enabled})", kafka.Enabled);

        while (!stopping.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var pending = await db.OutboxMessages
                    .Where(o => o.PublishedAt == null && o.RetryCount < 50)
                    .OrderBy(o => o.Id)
                    .Take(100)
                    .ToListAsync(stopping);

                foreach (var row in pending)
                {
                    try
                    {
                        var evt = System.Text.Json.JsonSerializer.Deserialize<NotificationEvent>(row.Payload);
                        if (evt is null) { row.PublishedAt = DateTime.UtcNow; continue; }

                        if (kafka.Enabled && kafka.TryProduce(evt))
                        {
                            row.PublishedAt = DateTime.UtcNow; // مصرف توسط NotificationConsumerJob
                        }
                        else if (!kafka.Enabled)
                        {
                            // dev بدون کافکا: مستقیم اعلان بساز + push زنده
                            db.Notifications.Add(new Notification
                            {
                                UserId = evt.UserId, Title = evt.Title, Body = evt.Body,
                                Icon = evt.Icon, Link = evt.Link, CreatedAt = DateTime.UtcNow
                            });
                            row.PublishedAt = DateTime.UtcNow;
                            await broadcaster.PushAsync(evt.UserId, evt.Title, evt.Body, evt.Icon, evt.Link, stopping);
                        }
                        else
                        {
                            row.RetryCount++;
                            row.LastError = "بروکر کافکا در دسترس نیست";
                        }
                    }
                    catch (Exception ex)
                    {
                        row.RetryCount++;
                        row.LastError = ex.Message;
                    }
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(stopping);

                // پاک‌سازی دوره‌ای ردیف‌های منتشرشدهٔ قدیمی
                if (Random.Shared.Next(100) < 5)
                {
                    await db.OutboxMessages
                        .Where(o => o.PublishedAt != null && o.PublishedAt < DateTime.UtcNow.AddDays(-7))
                        .ExecuteDeleteAsync(stopping);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stopping);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطا در OutboxDispatcher");
                try { await Task.Delay(TimeSpan.FromSeconds(10), stopping); } catch (OperationCanceledException) { break; }
            }
        }
    }
}

/// <summary>
/// مصرف‌کنندهٔ Kafka: رویدادهای اعلان را از تاپیک می‌خواند و در دیتابیس می‌نویسد.
/// فقط وقتی کافکا فعال است اجرا می‌شود؛ بروکر قطع → تلاش مجدد دوره‌ای.
/// </summary>
public class NotificationConsumerJob(
    KafkaProducerHolder kafka,
    IServiceScopeFactory scopes,
    INotificationBroadcaster broadcaster,
    ILogger<NotificationConsumerJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stopping)
    {
        if (!kafka.Enabled) return;

        while (!stopping.IsCancellationRequested)
        {
            try
            {
                using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
                {
                    BootstrapServers = kafka.BootstrapServers,
                    GroupId = "shop-notifications",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
                consumer.Subscribe(kafka.Topic);
                logger.LogInformation("NotificationConsumer به کافکا متصل شد ({Topic})", kafka.Topic);

                while (!stopping.IsCancellationRequested)
                {
                    var result = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result is null) continue;

                    try
                    {
                        var evt = System.Text.Json.JsonSerializer.Deserialize<NotificationEvent>(result.Message.Value);
                        if (evt is null) continue;

                        using var scope = scopes.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        db.Notifications.Add(new Notification
                        {
                            UserId = evt.UserId,
                            Title = evt.Title,
                            Body = evt.Body,
                            Icon = evt.Icon,
                            Link = evt.Link,
                            CreatedAt = DateTime.UtcNow
                        });
                        await db.SaveChangesAsync(stopping);

                        // اعلان زنده (SignalR)
                        try
                        {
                            await broadcaster.PushAsync(evt.UserId, evt.Title, evt.Body, evt.Icon, evt.Link, stopping);
                        }
                        catch (Exception brEx)
                        {
                            logger.LogWarning(brEx, "broadcast SignalR ناموفق بود");
                        }

                        // پیامک رویدادهای مهم (سفارش/پرداخت)
                        if (evt.Sms)
                        {
                            try
                            {
                                var sms = scope.ServiceProvider.GetRequiredService<ISmsService>();
                                var mobile = await db.Users.AsNoTracking().Where(u => u.Id == evt.UserId)
                                    .Select(u => u.Mobile).FirstOrDefaultAsync(stopping);
                                if (!string.IsNullOrWhiteSpace(mobile))
                                    await sms.SendAsync(mobile, $"{evt.Title}\n{(evt.Body ?? "")}".Trim(), stopping);
                            }
                            catch (Exception smsEx)
                            {
                                logger.LogWarning(smsEx, "ارسال SMS اعلان ناموفق بود");
                            }
                        }

                        consumer.Commit(result);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "خطا در پردازش رویداد اعلان");
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "اتصال به کافکا برقرار نشد — تلاش مجدد پس از ۳۰ ثانیه");
                try { await Task.Delay(TimeSpan.FromSeconds(30), stopping); } catch (OperationCanceledException) { break; }
            }
        }
    }
}
