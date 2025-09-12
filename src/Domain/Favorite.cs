using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Favorite
    {
        public Favorite()
        {
        }

        public int Id { get; set; }

        [Display(Name = "نام کاربر")]
        [DisplayName("نام کاربر")]
        public Guid UserId { get; set; }

        public virtual User Users { get; set; }

        [Display(Name = "نام کاربر")]
        [DisplayName("نام کاربر")]
        public long ProductId { get; set; }

        public virtual Product Product { get; set; }

        public DateTime? Date { get; set; }
    }
}
