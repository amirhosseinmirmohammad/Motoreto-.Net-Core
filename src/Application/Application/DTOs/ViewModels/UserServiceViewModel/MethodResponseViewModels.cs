using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ViewModels.UserServiceViewModel
{
    public class MethodResponseViewModels : object
    {
        public MethodResponseViewModels() { }
        public bool IsInRange { get; set; }
        public Order order { get; set; }
    }

    //public class AddsResponseViewModels : System.Object
    //{
    //    public AddsResponseViewModels() { }
    //    public bool IsInRange { get; set; }
    //    public Advertisement advertisement { get; set; }
    //}
}
