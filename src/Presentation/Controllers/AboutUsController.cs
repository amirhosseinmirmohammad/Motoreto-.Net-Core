using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AboutUsController : Controller
    {
        public AboutUsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /AboutUs
        public IActionResult Index()
        {
            var app = _db.Applications.FirstOrDefault();
            return View(app);
        }


        private readonly ApplicationDbContext _db;
    }
}
