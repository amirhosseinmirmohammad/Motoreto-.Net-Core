using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Slider
    {
        public int Id { get; set; }

        [DisplayName("آدرس تصویر")]
        [Display(Name = "آدرس تصویر")]
        public string Image { get; set; }

        [DisplayName("لینک")]
        [Display(Name = "لینک")]
        public string Link { get; set; }

        [DisplayName("تاریخ ثبت")]
        public DateTime? CreateDate { get; set; }

        [DisplayName("تاریخ به روز رسانی")]
        public DateTime? LastDate { get; set; }

        [DisplayName("نمایش در اپلیکیشن دارد ؟")]
        [Display(Name = "نمایش در اپلیکیشن دارد ؟")]
        public bool IsApp { get; set; }
    }
}
