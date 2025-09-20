using DatabaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace YourNamespace.Controllers
{
    public class JobRequirementDetailController : Controller
    {
        private JobSeacrhDbEntities db = new JobSeacrhDbEntities();

        // GET: JobRequirementDetail/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: JobRequirementDetail/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobRequirementDetailTable requirementDetail)
        {
            if (ModelState.IsValid)
            {
                // Assign current date
                //requirementDetail.JobRequirementDate = int;

                // Save to database
                db.JobRequirementDetailTables.Add(requirementDetail);
                db.SaveChanges();

                // Also save to TempData for immediate dropdown refresh
                var tempList = TempData["Requirements"] as List<JobRequirementDetailTable> ?? new List<JobRequirementDetailTable>();
                tempList.Add(requirementDetail);
                TempData["Requirements"] = tempList;

                // Redirect back to PostJob/Create
                return RedirectToAction("Create", "PostJob");
            }

            return View(requirementDetail);
        }
        public ActionResult Apply(int? postJobId)
        {
            if (!postJobId.HasValue)
                return RedirectToAction("", "JobSeeker"); // or show an error

            if (Session["UserID"] == null)
                return RedirectToAction("Login", "User");

            var job = db.PostJobTables.FirstOrDefault(j => j.PostJobID == postJobId.Value);
            if (job == null)
                return HttpNotFound();

            var apply = new JobApplysTable
            {
                PostJobID = postJobId.Value,
                EmployeeID = (int)Session["UserID"],
                JobApplyDateTime = DateTime.Now,
                JobApplyStatusID = 1,
                JobApplyStatusUpdateDate = DateTime.Now,
                JobApplyStatusUpdateReason = ""
            };

            ViewBag.JobTitle = job.JobTitle;

            return View(apply);
        }

    }
}
