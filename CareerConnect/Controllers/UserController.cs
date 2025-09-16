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
            // Correct binding for UserType dropdown
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

            // Validate UserTypeID exists
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
                    AccountStatusID = 1 // Default status
                };

                Db.UserTables.Add(userEntity);
                Db.SaveChanges(); // Auto-generates UserID

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("NewUser"); // Or redirect to login
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


        // GET: Login page
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

            // Check credentials
            var user = Db.UserTables
                .FirstOrDefault(u => u.UserName == loginMV.UserName && u.Password == loginMV.Password);

            if (user != null)
            {
                // Store user info in session
                Session["UserID"] = user.UserID;
                Session["UserName"] = user.UserName;
                Session["UserTypeID"] = user.UserTypeID;

                return RedirectToAction("Dashboard"); // Redirect after successful login
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(loginMV);
        }

        // Dashboard
        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            ViewBag.UserName = Session["UserName"];
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
