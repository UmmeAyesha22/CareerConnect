using DatabaseLayer;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity;

namespace YourNamespace.Controllers
{
    public class JobApplicationController : Controller
    {
        private JobSeacrhDbEntities db = new JobSeacrhDbEntities();


        // GET: JobApplication/Apply/5
        public ActionResult Apply(int? postJobId)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "User");

            if (postJobId == null)
                return RedirectToAction("Index", "JobSeeker");

            int jobId = postJobId.Value;
            int userId = (int)Session["UserID"];

            var job = db.PostJobTables.FirstOrDefault(j => j.PostJobID == jobId);
            if (job == null)
                return HttpNotFound();

            ViewBag.JobTitle = job.JobTitle;

            var existing = db.JobApplysTables
                             .Include(a => a.PostJobTable)
                             .Include(a => a.JobApplyStatusTable)
                             .FirstOrDefault(a => a.PostJobID == jobId && a.EmployeeID == userId);

            if (existing != null)
            {
                ViewBag.AlreadyApplied = true;
                ViewBag.Status = existing.JobApplyStatusTable?.JobApplyStatus ?? "Unknown";

                // Show existing application in the list
                var singleList = new List<JobApplysTable> { existing };
                return View("~/Views/JobApplication/AppliedJobs.cshtml", singleList);
            }

            var apply = new JobApplysTable
            {
                PostJobID = jobId,
                EmployeeID = userId,
                JobApplyDateTime = DateTime.Now,
                JobApplyStatusID = 1,
                JobApplyStatusUpdateDate = DateTime.Now
            };

            return View(apply);
        }

        // POST: JobApplication/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(JobApplysTable model)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "User");

            int userId = (int)Session["UserID"];
            model.EmployeeID = userId;
            model.JobApplyDateTime = DateTime.Now;
            model.JobApplyStatusID = 1;
            model.JobApplyStatusUpdateDate = DateTime.Now;

            var existing = db.JobApplysTables
                             .Include(a => a.PostJobTable)
                             .Include(a => a.JobApplyStatusTable)
                             .FirstOrDefault(a => a.PostJobID == model.PostJobID && a.EmployeeID == userId);

            if (existing != null)
            {
                ViewBag.JobTitle = existing.PostJobTable?.JobTitle ?? "";
                ViewBag.AlreadyApplied = true;

                var singleList = new List<JobApplysTable> { existing };
                // Use explicit path to avoid 404
                return View("~/Views/JobApplication/AppliedJobs.cshtml", singleList);
            }

            if (ModelState.IsValid)
            {
                db.JobApplysTables.Add(model);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Application submitted successfully!";

                // Redirect using full route to avoid 404
                return RedirectToAction("AppliedJobs", "JobApplication");
            }

            var job = db.PostJobTables.FirstOrDefault(j => j.PostJobID == model.PostJobID);
            ViewBag.JobTitle = job?.JobTitle ?? "";
            return View(model);
        }

        // GET: JobApplication/AppliedJobs
        public ActionResult AppliedJobs()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "User");

            int userId = (int)Session["UserID"];

            var applications = db.JobApplysTables
                                 .Include(a => a.PostJobTable)
                                 .Include(a => a.JobApplyStatusTable)
                                 .Where(a => a.EmployeeID == userId)
                                 .ToList();

            // Use explicit path to ensure view is found
            return View("~/Views/JobApplication/AppliedJobs.cshtml", applications);
        }

        public ActionResult JobApplications()
        {
            // 1️⃣ Check if logged in
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "User");

            int userId = (int)Session["UserID"];
            int userType = Session["UserTypeID"] != null ? (int)Session["UserTypeID"] : 0;

            // 2️⃣ Only Job Provider
            if (userType != 2)
                return RedirectToAction("Unauthorized", "User");

            // 3️⃣ Fetch applications for jobs posted by this provider
            var applications = db.JobApplysTables
                                 .Include(a => a.PostJobTable)
                                 .Include(a => a.JobApplyStatusTable)
                                 .Where(a => a.PostJobTable.UserID == userId)
                                 .ToList();

            // 4️⃣ Render view
            return View("~/Views/JobApplication/JobApplications.cshtml", applications);
        }

        // POST: Update Application Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(int jobApplyId, int statusId)
        {
            var application = db.JobApplysTables.Find(jobApplyId);
            if (application == null)
                return HttpNotFound();

            application.JobApplyStatusID = statusId;
            application.JobApplyStatusUpdateDate = DateTime.Now;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Application status updated successfully.";
            return RedirectToAction("JobApplications");
        }

    }
}


