using System.ComponentModel.DataAnnotations;

namespace Shop.Application.ViewModels;

/// <summary>فرم ورود.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = default!;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

/// <summary>فرم ثبت‌نام.</summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "نام را وارد کنید.")]
    [StringLength(50)]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
    [StringLength(50)]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "شماره موبایل را وارد کنید.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید با ۹ شروع شده و ۱۱ رقم باشد.")]
    [Display(Name = "شماره موبایل")]
    public string Mobile { get; set; } = default!;

    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "تکرار رمز عبور را وارد کنید.")]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    [DataType(DataType.Password)]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = default!;
}

/// <summary>ویرایش پروفایل.</summary>
public class ProfileViewModel
{
    [Required(ErrorMessage = "نام را وارد کنید.")]
    [StringLength(50)]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
    [StringLength(50)]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = default!;

    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل نامعتبر است.")]
    [Display(Name = "شماره موبایل")]
    public string? Mobile { get; set; }

    [Display(Name = "استان")]
    public int? StateId { get; set; }

    [Display(Name = "شهر")]
    public int? CityId { get; set; }

    [StringLength(300)]
    [Display(Name = "آدرس")]
    public string? AddressLine { get; set; }

    [Display(Name = "کد پستی")]
    public string? PostalCode { get; set; }
}

/// <summary>تغییر رمز عبور.</summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز عبور فعلی را وارد کنید.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور فعلی")]
    public string CurrentPassword { get; set; } = default!;

    [Required(ErrorMessage = "رمز عبور جدید را وارد کنید.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور جدید")]
    public string NewPassword { get; set; } = default!;

    [Required(ErrorMessage = "تکرار رمز عبور جدید را وارد کنید.")]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    [DataType(DataType.Password)]
    [Display(Name = "تکرار رمز عبور جدید")]
    public string ConfirmPassword { get; set; } = default!;
}

/// <summary>
/// فرم تسویه حساب. رزرو موجودی قبل از نمایش این فرم فعال شده است.
/// </summary>
public class CheckoutViewModel
{
    [Required(ErrorMessage = "نام گیرنده را وارد کنید.")]
    [StringLength(100)]
    [Display(Name = "نام و نام خانوادگی گیرنده")]
    public string ReceiverName { get; set; } = default!;

    [Required(ErrorMessage = "شماره تماس را وارد کنید.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل نامعتبر است.")]
    [Display(Name = "شماره تماس")]
    public string Phone { get; set; } = default!;

    [Required(ErrorMessage = "استان را انتخاب کنید.")]
    [Range(1, int.MaxValue, ErrorMessage = "استان را انتخاب کنید.")]
    [Display(Name = "استان")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "شهر را انتخاب کنید.")]
    [Range(1, int.MaxValue, ErrorMessage = "شهر را انتخاب کنید.")]
    [Display(Name = "شهر")]
    public int CityId { get; set; }

    [Required(ErrorMessage = "آدرس را وارد کنید.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "آدرس باید حداقل ۱۰ کاراکتر باشد.")]
    [Display(Name = "آدرس کامل")]
    public string Address { get; set; } = default!;

    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد پستی باید ۱۰ رقم باشد.")]
    [Display(Name = "کد پستی")]
    public string? PostalCode { get; set; }

    [StringLength(500)]
    [Display(Name = "توضیحات سفارش (اختیاری)")]
    public string? Note { get; set; }

    /// <summary>۱ = پست، ۲ = اکسپرس.</summary>
    [Range(1, 2, ErrorMessage = "روش ارسال را انتخاب کنید.")]
    public int ShippingMethod { get; set; } = 1;

    /// <summary>۱ = آنلاین، ۲ = پرداخت در محل.</summary>
    [Range(1, 2, ErrorMessage = "روش پرداخت را انتخاب کنید.")]
    public int PaymentMethod { get; set; } = 1;

    /// <summary>کد تخفیف (اختیاری).</summary>
    [StringLength(50)]
    public string? DiscountCode { get; set; }
}

/// <summary>ثبت نظر محصول.</summary>
public class CommentCreateViewModel
{
    [Range(1, 5, ErrorMessage = "امتیاز را انتخاب کنید.")]
    public byte Rating { get; set; } = 5;

    [Required(ErrorMessage = "متن نظر را وارد کنید.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "متن نظر باید حداقل ۱۰ کاراکتر باشد.")]
    public string Text { get; set; } = default!;
}

/// <summary>فرم تماس با ما.</summary>
public class ContactMessageViewModel
{
    [Required(ErrorMessage = "نام را وارد کنید.")]
    [StringLength(100)]
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; } = default!;

    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "موضوع را وارد کنید.")]
    [StringLength(150)]
    [Display(Name = "موضوع")]
    public string Subject { get; set; } = default!;

    [Required(ErrorMessage = "متن پیام را وارد کنید.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "متن پیام باید حداقل ۱۰ کاراکتر باشد.")]
    [Display(Name = "متن پیام")]
    public string Body { get; set; } = default!;
}
