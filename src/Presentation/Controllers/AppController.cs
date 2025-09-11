using Infrastructure;
using System.Web.Mvc;

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