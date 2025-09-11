using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class User : IdentityUser
    {
        [DisplayName("نام")]
        public string FirstName { get; set; }

        [DisplayName("نام خانوادگی")]
        public string LastName { get; set; }

        [DisplayName("جنسیت")]
        public byte? Gender { get; set; }

        [DisplayName("تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [DisplayName("تاریخ ثبت نام")]
        public DateTime RegistrationDate { get; set; }

        [DisplayName("امتیاز کاربر")]
        public long UserScore { get; set; }

        [DisplayName("تصویر پروفایل")]
        public string ProfileImage { get; set; }

        [DisplayName("آدرس کاربر")]
        public string AddressLine { get; set; }

        public string AccessCode { get; set; }

        public string IntroCode { get; set; }

        public string IntroCodeOptional { get; set; }

        [DisplayName("اعتبار")]
        public long Credit { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public string GenderText => Gender switch
        {
            1 => "مرد",
            2 => "زن",
            _ => "سایر"
        };

        [DisplayName("موبایل")]
        [RegularExpression(@"\b\d{4}[\s\-.]?\d{3}[\s\-.]?\d{4}\b", ErrorMessage = "فرمت تلفن همراه وارد شده صحیح نیست.")]
        public string Mobile { get; set; }

        public int? StateId { get; set; }

        public virtual State State { get; set; }

        public int? CityId { get; set; }

        public virtual City City { get; set; }

        public string VerificationCode { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }

        public virtual ICollection<Discount> Discounts { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }

        public string PlayerId { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }
    }
}
