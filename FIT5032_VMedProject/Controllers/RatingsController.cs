using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FIT5032_VMedProject.Models;

namespace FIT5032_VMedProject.Controllers
{
    public class RatingsController : Controller
    {
        private Entities2 db = new Entities2();

        // GET: Ratings
        [Authorize]
        public ActionResult Index()
        {
            var ratings = db.Ratings.Include(r => r.Doctor);
            return View(ratings.ToList());
        }

        // GET: Ratings/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            return View(rating);
        }

        // GET: Ratings/ViewRatingsDetails/5
        [Authorize]
        public ActionResult ViewRatingsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            return View(rating);
        }

        // GET: AllRatings
        [Authorize]
        public ActionResult AllRatings()
        {
            // Redirect to the "Index" action in the "DoctorsController"
            return RedirectToAction("Index", "Doctors");
        }

        // GET: Ratings of a specific doctor
        [Authorize]
        public ActionResult ViewRatings(int doctorId)
        {
            // Retrieve the ratings for the specified doctorId
            var ratings = db.Ratings.Where(r => r.DoctorID == doctorId).ToList();

            // You can also retrieve the doctor's information here if needed

            return View(ratings);
        }

        // GET: Ratings/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName");
            return View();
        }

        // POST: Ratings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "RatingID,DoctorID,RatingValue,Comment")] Rating rating)
        {
            if (ModelState.IsValid)
            {
                db.Ratings.Add(rating);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // GET: Ratings/ViewRatingsCreate
        [Authorize]
        public ActionResult ViewRatingsCreate(int? doctorId)
        {
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName");
            // Set the doctorId in ViewBag
            ViewBag.SelectedDoctorID = doctorId;
            return View();
        }

        // POST: Ratings/ViewRatingsCreate
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ViewRatingsCreate([Bind(Include = "RatingID,DoctorID,RatingValue,Comment")] Rating rating)
        {
            // Access the doctorId from the rating object
            int doctorId = rating.DoctorID;

            if (ModelState.IsValid)
            {
                db.Ratings.Add(rating);
                db.SaveChanges();
                return RedirectToAction("ViewRatings", new { doctorId = doctorId });
            }

            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // GET: Ratings/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // POST: Ratings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "RatingID,DoctorID,RatingValue,Comment")] Rating rating)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rating).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // GET: Ratings/ViewRatingsEdit/5
        [Authorize]
        public ActionResult ViewRatingsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // POST: Ratings/ViewRatingsEdit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ViewRatingsEdit([Bind(Include = "RatingID,DoctorID,RatingValue,Comment")] Rating rating)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rating).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ViewRatings", new { doctorId = rating.DoctorID });
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", rating.DoctorID);
            return View(rating);
        }

        // GET: Ratings/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            return View(rating);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Rating rating = db.Ratings.Find(id);
            db.Ratings.Remove(rating);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Ratings/ViewRatingsDelete/5
        [Authorize]
        public ActionResult ViewRatingsDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rating rating = db.Ratings.Find(id);
            if (rating == null)
            {
                return HttpNotFound();
            }
            return View(rating);
        }

        // POST: Ratings/ViewRatingsDeleteConfirmed/5
        [HttpPost, ActionName("ViewRatingsDelete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ViewRatingsDeleteConfirmed(int id)
        {
            Rating rating = db.Ratings.Find(id);
            db.Ratings.Remove(rating);
            db.SaveChanges();
            return RedirectToAction("ViewRatings", new { doctorId = rating.DoctorID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
