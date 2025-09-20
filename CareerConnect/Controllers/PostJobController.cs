using DatabaseLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CareerConnect.Controllers
{
    public class PostJobController : Controller
    {
        private JobSeacrhDbEntities db = new JobSeacrhDbEntities();

        // GET: PostJob/Index - list of jobs for logged-in job provider
        public ActionResult Index()
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            int userType = Convert.ToInt32(Session["UserTypeID"]);

            IQueryable<PostJobTable> jobsQuery = db.PostJobTables
                .Include(j => j.JobCategoryTable)
                .Include(j => j.JobStatusTable)
                .Include(j => j.JobRequirementsTable)
                .Include(j => j.CompanyTable);

            if (userType == 2) // Job Provider
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                jobsQuery = jobsQuery.Where(j => j.UserID == userId);
            }
            else if (userType != 1) // Not Admin or Job Provider
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are not authorized.");
            }

            var jobs = jobsQuery.ToList();
            return View(jobs);
        }



        public ActionResult AvailableJobs(int? categoryId, int? natureId)
        {
            // Load dropdown lists for filters
            ViewBag.Categories = db.JobCategoryTables.ToList();
            ViewBag.Natures = db.JobNatureTables.ToList();

            // Preserve selected filters
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedNature = natureId;

            // Base query: approved jobs that are not expired
            var today = DateTime.Today;
            var jobsQuery = db.PostJobTables
                              .Where(j => j.JobStatusID == 2 && j.ApplicationLastDate >= today)
                              .AsQueryable();

            // Apply category filter if selected
            if (categoryId.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.JobCategoryID == categoryId.Value);
            }

            // Apply job nature filter if selected
            if (natureId.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.JobNatureID == natureId.Value);
            }

            // Fetch jobs with related data
            var jobs = jobsQuery
                        .OrderByDescending(j => j.ApplicationLastDate)
                        .ToList();

            return View(jobs);
        }

        // GET: PostJob/Apply?jobId=xxx
        // Redirects to JobApplicationController.Create for applying
        public ActionResult Apply(int jobId)
        {
            // Optional: check if job exists and is open
            var job = db.PostJobTables.FirstOrDefault(j => j.PostJobID == jobId && j.JobStatusID == 2 && j.ApplicationLastDate >= DateTime.Today);
            if (job == null)
            {
                TempData["Error"] = "This job is no longer available for application.";
                return RedirectToAction("AvailableJobs");
            }

            // Redirect to JobApplication creation page
            return RedirectToAction("Create", "JobApplication", new { jobId = jobId });
        }

        // Dispose DbContext properly
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult JobList()
        {
            return RedirectToAction("AvailableJobs");
        }


        // GET: PostJob/Create
        public ActionResult Create()
        {
            // 🔹 Redirect to login if session expired
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            // 🔹 Only Job Provider (UserTypeID = 2) can create jobs
            if (Convert.ToInt32(Session["UserTypeID"]) != 2)
                return View("Unauthorized"); // friendly error page

            LoadDropdowns();
            return View();
        }

        // POST: PostJob/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PostJobTable postJob)
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            if (Convert.ToInt32(Session["UserTypeID"]) != 2)
                return View("Unauthorized");

            // Validate application last date
            if (postJob.ApplicationLastDate < DateTime.Now)
                ModelState.AddModelError("ApplicationLastDate", "Application last date cannot be in the past.");

            if (ModelState.IsValid)
            {
                try
                {
                    postJob.UserID = Convert.ToInt32(Session["UserID"]);
                    postJob.PostDate = DateTime.Now;

                    // Default status = Pending
                    postJob.JobStatusID = db.JobStatusTables
                        .Where(s => s.JobStatus == "Pending")
                        .Select(s => s.JobStatusID)
                        .FirstOrDefault();

                    db.PostJobTables.Add(postJob);
                    db.SaveChanges();

                    // Redirect to job list after saving
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                            ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving job: " + ex.Message);
                }
            }

            // Reload dropdowns if validation fails
            LoadDropdowns(postJob);
            return View(postJob);
        }

        // Helper method for dropdowns
        private void LoadDropdowns(PostJobTable postJob = null)
        {
            ViewBag.CompanyID = new SelectList(db.CompanyTables.ToList(), "CompanyID", "CompanyName", postJob?.CompanyID);
            ViewBag.JobCategoryID = new SelectList(db.JobCategoryTables.ToList(), "JobCategoryID", "JobCategory", postJob?.JobCategoryID);
            ViewBag.JobNatureID = new SelectList(db.JobNatureTables.ToList(), "JobNatureID", "JobNature", postJob?.JobNatureID);
            // Load requirements from TempData if available
            var tempRequirements = TempData["Requirements"] as List<JobRequirementDetailTable> ?? new List<JobRequirementDetailTable>();

            // Load requirements from database
            var dbRequirements = db.JobRequirementDetailTables.ToList();

            // Combine both lists, avoiding duplicates
            var allRequirements = tempRequirements
                .Concat(dbRequirements)
                .GroupBy(r => r.JobRequirementID)
                .Select(g => g.First())
                .ToList();

            // Keep TempData for next request
            TempData.Keep("Requirements");

            // Set dropdown
            ViewBag.JobRequirementsID = new SelectList(
                allRequirements,
                "JobRequirementID",
                "JobRequirementDetails",
                postJob?.JobRequirementsID
            );

        }

        // POST: Update job status (Admin only)
        [HttpPost]
        public ActionResult UpdateStatus(int jobId, int statusId)
        {
            if (Session["UserTypeID"] != null && (int)Session["UserTypeID"] == 1)
            {
                var job = db.PostJobTables.Find(jobId);
                if (job != null)
                {
                    job.JobStatusID = statusId;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }


        // GET: PostJob/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var job = db.PostJobTables.Find(id);
            if (job == null)
                return HttpNotFound();

            int userType = Convert.ToInt32(Session["UserTypeID"]);
            int userId = Convert.ToInt32(Session["UserID"]);

            // Only Admin or Job Provider who owns the job
            if (!( (userType == 2 && job.UserID == userId)))
                return View("Unauthorized");

            PopulateJobDropdowns(job); // load dropdowns with current values
            return View(job);
        }

        // POST: PostJob/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PostJobTable postJob)
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            int userType = Convert.ToInt32(Session["UserTypeID"]);
            int userId = Convert.ToInt32(Session["UserID"]);

            var existingJob = db.PostJobTables.Find(postJob.PostJobID);
            if (existingJob == null)
                return HttpNotFound();

            // Only Admin or Job Provider who owns the job
            if (!((userType == 2 && existingJob.UserID == userId)))
                return View("Unauthorized");

            if (ModelState.IsValid)
            {
                existingJob.JobTitle = postJob.JobTitle;
                existingJob.CompanyID = postJob.CompanyID;
                existingJob.JobCategoryID = postJob.JobCategoryID;
                existingJob.Location = postJob.Location;
                existingJob.MinSalary = postJob.MinSalary;
                existingJob.MaxSalary = postJob.MaxSalary;
                existingJob.Vacancy = postJob.Vacancy;
                existingJob.JobNatureID = postJob.JobNatureID;
                existingJob.JobRequirementsID = postJob.JobRequirementsID;
                existingJob.ApplicationLastDate = postJob.ApplicationLastDate;

                db.SaveChanges();

                // Set a TempData token for success message
                TempData["Success"] = "Job updated successfully!";
                return RedirectToAction("Index"); // redirect to PostJob Index page
            }

            PopulateJobDropdowns(postJob); // reload dropdowns if validation fails
            return View(postJob);
        }

        // Dropdowns method
        private void PopulateJobDropdowns(PostJobTable job = null)
        {
            ViewBag.CompanyID = new SelectList(db.CompanyTables.ToList(), "CompanyID", "CompanyName", job?.CompanyID);
            ViewBag.JobCategoryID = new SelectList(db.JobCategoryTables.ToList(), "JobCategoryID", "JobCategory", job?.JobCategoryID);
            ViewBag.JobNatureID = new SelectList(db.JobNatureTables.ToList(), "JobNatureID", "JobNature", job?.JobNatureID);
            ViewBag.JobRequirementsID = new SelectList(db.JobRequirementsTables.ToList(), "JobRequirementID", "JobRequirementTitle", job?.JobRequirementsID);
        }



        // GET: PostJob/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var job = db.PostJobTables.Find(id);
            if (job == null)
                return HttpNotFound();

            int userType = Convert.ToInt32(Session["UserTypeID"]);
            int userId = Convert.ToInt32(Session["UserID"]);

            // Only Admin or Job Provider who owns the job
            if (!( (userType == 2 && job.UserID == userId)))
                return View("Unauthorized");

            return View(job); // confirm deletion page
        }

        // POST: PostJob/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["UserID"] == null || Session["UserTypeID"] == null)
                return RedirectToAction("Login", "User");

            var job = db.PostJobTables.Find(id);
            if (job == null)
                return HttpNotFound();

            int userType = Convert.ToInt32(Session["UserTypeID"]);
            int userId = Convert.ToInt32(Session["UserID"]);

            // Only Job Provider who owns the job
            if (!(userType == 2 && job.UserID == userId))
                return View("Unauthorized");

            // Check if job has applications
            bool hasApplications = db.JobApplysTables.Any(a => a.PostJobID == id);
            if (hasApplications)
            {
                TempData["Error"] = "This job cannot be deleted because there are applications linked to it.";
                return RedirectToAction("Index");
            }

            db.PostJobTables.Remove(job);
            db.SaveChanges();

            TempData["Success"] = "Job deleted successfully!";
            return RedirectToAction("Index");
        }





    }
}
