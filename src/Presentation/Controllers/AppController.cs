using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AppController : Controller
    {
        public AppController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: App
        public ActionResult Index()
        {
            return View();
        }


        private readonly ApplicationDbContext _db;
    }
}