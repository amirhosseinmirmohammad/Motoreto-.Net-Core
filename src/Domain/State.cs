using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class State
    {
        public State()
        {
            Cities = new List<City>();
            Users = new List<User>();
        }

        public int Id { get; set; }

        [DisplayName("نام استان")]
        [Required(ErrorMessage = "لطفا نام استان را وارد کنید")]
        [MaxLength(100, ErrorMessage = "نام استان حداکثر می تواند شامل 100 کاراکتر باشد")]
        public string Name { get; set; }

        public virtual ICollection<City> Cities { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}