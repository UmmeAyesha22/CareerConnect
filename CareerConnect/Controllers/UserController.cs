using CareerConnect.Models;
using DatabaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CareerConnect.Controllers
{
    public class UserController : Controller
    {
        private JobSeacrhDbEntities Db = new JobSeacrhDbEntities();
        // GET: User
        public ActionResult NewUser()
        {
            ViewBag.UserTypeID = new SelectList(Db.UserTypeTables.Where(u => u.UserTypeID != 1).ToList(), "UserTypeID", "UserType", "0");
            return View(new UserMV());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewUser(UserMV userMV)
        {
            ViewBag.UserTypeID = new SelectList(Db.UserTypeTables.Where(u => u.UserTypeID != 1).ToList(), "UserTypeID", "UserType",userMV.UserTypeID);
            return View(userMV);
        }
    }
}