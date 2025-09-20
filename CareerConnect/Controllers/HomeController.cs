using DatabaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

    
    //public ActionResult Dashboard()
    //    {
    //        int userType = Session["UserType"] != null ? (int)Session["UserType"] : 0;
    //        int userId = Session["UserID"] != null ? (int)Session["UserID"] : 0;

    //        if (userType == 2) // Job Provider
    //        {
    //            var myJobs = db.PostJobTables.Where(j => j.UserID == userId).ToList();
    //            ViewBag.MyJobs = myJobs;
    //        }

    //        ViewBag.UserType = userType;
    //        return View();
    //    }


    }
}