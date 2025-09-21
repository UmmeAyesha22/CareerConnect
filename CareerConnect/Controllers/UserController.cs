using System.Linq;
using System.Web.Mvc;
using CareerConnect.Models;
using DatabaseLayer;

namespace CareerConnect.Controllers
{
    public class UserController : Controller
    {
        private JobSeacrhDbEntities Db = new JobSeacrhDbEntities();

        // GET: Registration form
        public ActionResult NewUser()
        {
            ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType");
            return View();
        }

        // POST: Save user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewUser(UserMV userMV)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType", userMV.UserTypeID);
                return View(userMV);
            }

            if (!Db.UserTypeTables.Any(u => u.UserTypeID == userMV.UserTypeID))
            {
                ModelState.AddModelError("UserTypeID", "Invalid user type selected.");
                ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType", userMV.UserTypeID);
                return View(userMV);
            }

            try
            {
                var userEntity = new UserTable
                {
                    UserTypeID = userMV.UserTypeID,
                    UserName = userMV.UserName,
                    Password = userMV.Password,
                    EmailAddress = userMV.EmailAddress,
                    ContactNo = userMV.ContactNo,
                    AccountStatusID = 1
                };

                Db.UserTables.Add(userEntity);
                Db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("NewUser");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                    }
                }
                ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType", userMV.UserTypeID);
                return View(userMV);
            }
        }

        // ✅ GET: Login page
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginMV loginMV)
        {
            if (!ModelState.IsValid)
                return View(loginMV);

            string username = loginMV.UserName.Trim();
            string password = loginMV.Password.Trim();

            var user = Db.UserTables.FirstOrDefault(u => u.UserName == username && u.Password == password);

            if (user != null)
            {
                Session["UserID"] = user.UserID;
                Session["UserName"] = user.UserName;
                Session["UserTypeID"] = user.UserTypeID;

                // Redirect all users to Home
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(loginMV);
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // User Profile
        public ActionResult UserProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserID"];
            var user = Db.UserTables.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
                return RedirectToAction("Login");

            var model = new UserMV
            {
                UserID = user.UserID,
                UserName = user.UserName,
                EmailAddress = user.EmailAddress,
                ContactNo = user.ContactNo,
                UserTypeID = user.UserTypeID,
                UserTypeName = Db.UserTypeTables
                        .Where(u => u.UserTypeID == user.UserTypeID)
                        .Select(u => u.UserType)
                        .FirstOrDefault()
            };

            return View(model);
        }
        // GET: AllUsers
        public ActionResult AllUsers()
        {
            // Only allow admins
            if (Session["UserTypeID"] == null || (int)Session["UserTypeID"] != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = Db.UserTables.Select(u => new UserMV
            {
                UserID = u.UserID,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress,
                ContactNo = u.ContactNo,
                UserTypeID = u.UserTypeID,
                UserTypeName = Db.UserTypeTables
                                .Where(t => t.UserTypeID == u.UserTypeID)
                                .Select(t => t.UserType)
                                .FirstOrDefault()
            }).ToList();

            return View(users);
        }

        public ActionResult Edit()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserID"];
            var user = Db.UserTables.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
                return RedirectToAction("Login");

            ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType", user.UserTypeID);

            var model = new UserMV
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Password = user.Password,
                EmailAddress = user.EmailAddress,
                ContactNo = user.ContactNo,
                UserTypeID = user.UserTypeID
            };

            return View(model); // This will now correctly find Edit.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserMV model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserTypeID = new SelectList(Db.UserTypeTables, "UserTypeID", "UserType", model.UserTypeID);
                return View(model);
            }

            var user = Db.UserTables.FirstOrDefault(u => u.UserID == model.UserID);
            if (user == null)
                return HttpNotFound();

            // Update fields
            user.UserName = model.UserName;
            user.Password = model.Password;
            user.EmailAddress = model.EmailAddress;
            user.ContactNo = model.ContactNo;
            user.UserTypeID = model.UserTypeID;

            Db.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("UserProfile");
        }



    }
}
