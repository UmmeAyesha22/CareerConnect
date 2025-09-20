using CareerConnect.Models;
using DatabaseLayer;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CareerConnect.Controllers
{
    public class CompanyController : Controller
    {
        private JobSeacrhDbEntities db = new JobSeacrhDbEntities();

        // GET: Company
        public ActionResult Index()
        {
            var companies = db.CompanyTables.ToList();
            return View(companies);
        }

        // GET: Company/Create
        [HttpGet]
        public ActionResult Create()
        {
            // Only Job Providers (UserTypeID = 2)
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            if ((int)Session["UserTypeID"] != 2)
            {
                TempData["Error"] = "Only Job Providers can create companies.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyMV model, HttpPostedFileBase LogoFile)
        {
            try
            {
                if (Session["UserID"] == null || Session["UserTypeID"] == null)
                    return RedirectToAction("Login", "User");

                int userId = (int)Session["UserID"];
                int userType = (int)Session["UserTypeID"];
                if (userType != 2)
                {
                    TempData["Error"] = "Only Job Providers can create companies.";
                    return RedirectToAction("Index");
                }

                // Ensure folder exists
                string folderPath = Server.MapPath("~/Content/assets/img/company/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string logoPath = "/Content/assets/img/company/default-logo.png";

                if (LogoFile != null && LogoFile.ContentLength > 0)
                {
                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(LogoFile.FileName)}";
                    string path = Path.Combine(folderPath, fileName);
                    LogoFile.SaveAs(path);
                    logoPath = "/Content/assets/img/company/" + fileName;
                }

                var company = new CompanyTable
                {
                    UserID = userId,
                    CompanyName = model.CompanyName,
                    ContactNo = model.ContactNo,
                    PhoneNo = model.PhoneNo,
                    EmailAddress = model.EmailAddress,
                    Description = model.Description,
                    Logo = logoPath
                };

                db.CompanyTables.Add(company);
                db.SaveChanges();

                TempData["Success"] = "Company created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return View(model);
            }
        }

        private bool IsAuthorized()
        {
            return Session["UserTypeID"] != null && ((int)Session["UserTypeID"] == 1 || (int)Session["UserTypeID"] == 2);
        }

        // Example in Edit GET
        public ActionResult Edit(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Index");

            var company = db.CompanyTables.Find(id);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        // Same check in Edit POST and Delete actions


        // POST: Company/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyTable model, HttpPostedFileBase LogoFile)
        {
            var company = db.CompanyTables.Find(model.CompanyID);
            if (company == null) return HttpNotFound();

            company.CompanyName = model.CompanyName;
            company.ContactNo = model.ContactNo;
            company.PhoneNo = model.PhoneNo;
            company.EmailAddress = model.EmailAddress;
            company.Description = model.Description;

            // Ensure folder exists
            string folderPath = Server.MapPath("~/Content/assets/img/company/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (LogoFile != null && LogoFile.ContentLength > 0)
            {
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(LogoFile.FileName)}";
                string path = Path.Combine(folderPath, fileName);
                LogoFile.SaveAs(path);
                company.Logo = "/Content/assets/img/company/" + fileName;
            }

            db.SaveChanges();
            TempData["Success"] = "Company updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Company/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var company = db.CompanyTables.Find(id);
            if (company == null)
                return HttpNotFound();

            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var company = db.CompanyTables.Find(id);
            if (company != null)
            {
                // Delete logo file if not default
                if (!string.IsNullOrEmpty(company.Logo) &&
                    company.Logo != "/Content/assets/img/company/default-logo.png")
                {
                    string filePath = Server.MapPath(company.Logo);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                db.CompanyTables.Remove(company);
                db.SaveChanges();
            }

            TempData["Success"] = "Company deleted successfully!";
            return RedirectToAction("Index");
        }


    }

}
