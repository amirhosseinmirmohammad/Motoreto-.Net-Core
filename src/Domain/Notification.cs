using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Notification
    {
        public Notification()
        {
            Users = new List<User>();
            Images = new List<Image>();
            NewsLetters = new List<NewsLetter>();
        }

        public int Id { get; set; }

        [DisplayName("تاریخ ارسال")]
        [Display(Name = "تاریخ ارسال")]
        public DateTime ForwardDate { get; set; }

        [DisplayName("متن پیام")]
        [Display(Name = "متن پیام")]
        [Required(ErrorMessage ="لطفا متن پیام خود را تعیین نمایید .")]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }     

        public virtual ICollection<Image> Images { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<NewsLetter> NewsLetters { get; set; }
    }
}
