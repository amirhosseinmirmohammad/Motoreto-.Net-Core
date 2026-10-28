using System.ComponentModel.DataAnnotations;

namespace Shop.Application.ViewModels;

/// <summary>
/// فرم کامل ثبت‌نام فروشگاه در مارکت‌پلیس — حقیقی یا حقوقی، با مدارک.
/// اعتبارسنجی شرطی (بر اساس نوع شخصیت) در SellerApplyViewModel.Validate انجام می‌شود.
/// </summary>
public class SellerApplyViewModel : IValidatableObject
{
    // ---- نوع شخصیت ----

    /// <summary>۱ = حقیقی، ۲ = حقوقی.</summary>
    [Range(1, 2, ErrorMessage = "نوع شخصیت را انتخاب کنید.")]
    public int Type { get; set; } = 1;

    // ---- اطلاعات فروشگاه ----

    [Required(ErrorMessage = "نام فروشگاه الزامی است.")]
    [StringLength(150)]
    [Display(Name = "نام فروشگاه")]
    public string StoreName { get; set; } = default!;

    [StringLength(2000)]
    [Display(Name = "درباره فروشگاه")]
    public string? Description { get; set; }

    [StringLength(400)]
    [Display(Name = "آدرس فروشگاه / انبار")]
    public string? Address { get; set; }

    [StringLength(20)]
    [Display(Name = "تلفن فروشگاه")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    /// <summary>شماره شبا (بدون IR یا با IR).</summary>
    [RegularExpression(@"^(IR)?\d{24}$", ErrorMessage = "شماره شبا باید ۲۴ رقم باشد.")]
    [Display(Name = "شماره شبا (تسویه فروش)")]
    public string? Sheba { get; set; }

    // ---- شخص حقیقی ----

    [StringLength(50)]
    [Display(Name = "نام")]
    public string? FirstName { get; set; }

    [StringLength(50)]
    [Display(Name = "نام خانوادگی")]
    public string? LastName { get; set; }

    [StringLength(10)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")]
    [Display(Name = "کد ملی")]
    public string? NationalCode { get; set; }

    // ---- شخص حقوقی ----

    [StringLength(150)]
    [Display(Name = "نام شرکت")]
    public string? CompanyName { get; set; }

    [StringLength(11)]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "شناسه ملی باید ۱۱ رقم باشد.")]
    [Display(Name = "شناسه ملی شرکت")]
    public string? CompanyNationalId { get; set; }

    [StringLength(20)]
    [Display(Name = "کد اقتصادی")]
    public string? EconomicCode { get; set; }

    [StringLength(20)]
    [Display(Name = "شماره ثبت شرکت")]
    public string? RegistrationNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "نام مدیرعامل")]
    public string? CeoName { get; set; }

    [StringLength(10)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی مدیرعامل باید ۱۰ رقم باشد.")]
    [Display(Name = "کد ملی مدیرعامل")]
    public string? CeoNationalId { get; set; }

    // ---- اطلاعات مجوز ----

    [Required(ErrorMessage = "شماره جواز کسب / شماره ثبت شرکت الزامی است.")]
    [StringLength(80)]
    [Display(Name = "شماره جواز کسب / شماره ثبت")]
    public string BusinessLicenseNumber { get; set; } = default!;

    /// <summary>دسته‌هایی که فروشنده در آن‌ها فعالیت می‌کند (چند انتخابی) — از فرم Blazor پر می‌شود.</summary>
    public List<int> CategoryIds { get; set; } = [];

    // ---- مدارک (مسیر پس از آپلود در کنترلر) ----

    public string? BusinessLicenseFileUrl { get; set; }
    public string? NationalCardFileUrl { get; set; }
    public string? CompanyAdFileUrl { get; set; }

    // ---- اعتبارسنجی شرطی ----

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Type == 1) // حقیقی
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                yield return new ValidationResult("نام الزامی است.", [nameof(FirstName)]);
            if (string.IsNullOrWhiteSpace(LastName))
                yield return new ValidationResult("نام خانوادگی الزامی است.", [nameof(LastName)]);
            if (string.IsNullOrWhiteSpace(NationalCode) || NationalCode.Length != 10)
                yield return new ValidationResult("کد ملی ۱۰ رقمی الزامی است.", [nameof(NationalCode)]);
        }
        else // حقوقی
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
                yield return new ValidationResult("نام شرکت الزامی است.", [nameof(CompanyName)]);
            if (string.IsNullOrWhiteSpace(CompanyNationalId) || CompanyNationalId.Length != 11)
                yield return new ValidationResult("شناسه ملی ۱۱ رقمی شرکت الزامی است.", [nameof(CompanyNationalId)]);
            if (string.IsNullOrWhiteSpace(CeoName))
                yield return new ValidationResult("نام مدیرعامل الزامی است.", [nameof(CeoName)]);
        }
    }
}
