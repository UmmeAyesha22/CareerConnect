using DatabaseLayer;
using System.Web.Mvc;

namespace CareerConnect.Controllers
{
    public class HomeController : Controller
    {
        private JobSeacrhDbEntities db = new JobSeacrhDbEntities();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View(); // Views/Home/Contact.cshtml
        }

        // ✅ Extra methods for navigation menu
        public ActionResult Blog()
        {
            return View(); // Views/Home/Blog.cshtml
        }

        public ActionResult BlogDetails()
        {
            return View(); // Views/Home/BlogDetails.cshtml
        }

        public ActionResult Elements()
        {
            return View(); // Views/Home/Elements.cshtml
        }

        public ActionResult JobDetails()
        {
            return View(); // Views/Home/JobDetails.cshtml
        }
    }
}
