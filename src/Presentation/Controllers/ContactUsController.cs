using Infrastructure;
using System.Web.Mvc;

namespace Presentation.Controllers
{
    public class ContactUsController : Controller
    {
        public ContactUsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: ContactUs
        public ActionResult Index()
        {
            return View();
        }


        private readonly ApplicationDbContext _db;
    }
}