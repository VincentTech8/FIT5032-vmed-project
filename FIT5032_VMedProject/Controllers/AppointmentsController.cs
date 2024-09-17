using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FIT5032_VMedProject.Models;
using Microsoft.AspNet.Identity;

namespace FIT5032_VMedProject.Controllers
{
    public class AppointmentsController : Controller
    {
        private Entities2 db = new Entities2();

        // GET: Appointments
        [Authorize]
        public ActionResult Index()
        {
            var userObj = User.Identity;
            var userId = User.Identity.GetUserId();

            bool isAdmin = User.IsInRole("Administrator");

            // Conditionally filter based on the user's role
            IQueryable<Appointment> appointments;

            if (isAdmin)
            {
                // If the user is an admin, no filtering by UserId
                appointments = db.Appointments.Include(a => a.Doctor).Include(a => a.Service).Include(a => a.AspNetUser);
            }
            else
            {
                // If the user is not an admin, filter by UserId
                appointments = db.Appointments.Where(a => a.UserId == userId)
                                             .Include(a => a.Doctor)
                                             .Include(a => a.Service)
                                             .Include(a => a.AspNetUser);
            }

            return View(appointments.ToList());
        }

        // GET: Appointments/RecordView
        [Authorize]
        public ActionResult RecordView()
        {
            var userObj = User.Identity;
            var userId = User.Identity.GetUserId();

            bool isAdmin = User.IsInRole("Administrator");

            // Define a DateTime variable for the current date
            DateTime currentDate = DateTime.Now;

            // Conditionally filter based on the user's role and past appointments
            IQueryable<Appointment> appointments;

            if (isAdmin)
            {
                // If the user is an admin, no filtering by UserId
                appointments = db.Appointments
                    .Where(a => a.AppointmentDate < currentDate) // Filter past appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .Include(a => a.AspNetUser);
            }
            else
            {
                // If the user is not an admin, filter by UserId and past appointments
                appointments = db.Appointments
                    .Where(a => a.UserId == userId && a.AppointmentDate < currentDate) // Filter past appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .Include(a => a.AspNetUser);
            }

            var serviceCounts = appointments.GroupBy(a => a.Service.ServiceName)
                                            .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                                            .ToList();

            ViewBag.ServiceCounts = serviceCounts;

            return View(appointments.ToList());
        }

        // GET: Appointments/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName");
            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName");
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "AppointmentID,DoctorID,ServiceID,UserId,AppointmentDate,Notes")] Appointment appointment)
        {
            appointment.UserId = User.Identity.GetUserId();

            // Check if an appointment with the same date already exists for the current user
            var existingAppointment = db.Appointments.FirstOrDefault(a => a.AppointmentDate == appointment.AppointmentDate);

            // Return error if the appointment date has been used in another appointment
            if (existingAppointment != null)
            {
                ModelState.AddModelError("AppointmentDate", "An appointment with the same date already exists.");
            }

            if (ModelState.IsValid)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", appointment.DoctorID);
            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", appointment.UserId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", appointment.DoctorID);
            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", appointment.UserId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "AppointmentID,DoctorID,ServiceID,UserId,AppointmentDate,Notes")] Appointment appointment)
        {
            // Check if an appointment with the same date already exists for the current user
            var existingAppointment = db.Appointments.FirstOrDefault(a => a.AppointmentDate == appointment.AppointmentDate);

            // Return error if the appointment date has been used in another appointment
            if (existingAppointment != null)
            {
                ModelState.AddModelError("AppointmentDate", "An appointment with the same date already exists.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FirstName", appointment.DoctorID);
            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", appointment.UserId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("Index");
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
