using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class TermsController : Controller
    {
        // GET: Terms
        public ActionResult Index()
        {
            Domain.Application app = _db.Applications.FirstOrDefault();
            return View(app);
        }


        private readonly ApplicationDbContext _db;
    }
}