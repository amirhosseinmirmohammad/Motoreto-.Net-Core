using Infrastructure;
using System.Web.Mvc;

namespace Presentation.Controllers
{
    public class AboutUsController : Controller
    {
        public AboutUsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: AboutUs
        public ActionResult Index()
        {
            Domain.Application app = _db.Applications.FirstOrDefault();
            return View(app);
        }


        private readonly ApplicationDbContext _db;
    }
}