using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        public ActionResult Index()
        {
            return RedirectToAction("BookingPage", "Main");
        }

        [HttpPost]
        public ActionResult Create(
            int customerId,
            int serviceId,
            string bookingDate,
            string startTime,
            string endTime,
            string nailTech = null,
            string downpayment = null,
            string notes = null
        )
        {
            DateTime date;
            var dateFormats = new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };

            if (!DateTime.TryParseExact(bookingDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out date))
            {
                if (!DateTime.TryParse(bookingDate, out date))
                    return Json(new { success = false, message = "Invalid bookingDate." });
            }

            if (!TryParseTime(startTime, out TimeSpan st))
                return Json(new { success = false, message = "Invalid startTime." });

            if (!TryParseTime(endTime, out TimeSpan et))
                return Json(new { success = false, message = "Invalid endTime." });

            if (et <= st)
                return Json(new { success = false, message = "EndTime must be after StartTime." });

            var conflict = db.tbl_bookings.Any(b =>
                b.BookingDate == date.Date &&
                (b.Status == "Pending" || b.Status == "Approved") &&
                !(et <= b.StartTime || st >= b.EndTime)
            );

            if (conflict)
                return Json(new { success = false, message = "Time slot already taken." });

            var finalNotes = notes ?? "";
            if (!string.IsNullOrWhiteSpace(nailTech))
                finalNotes = (finalNotes + " | NailTech: " + nailTech).Trim();

            if (!string.IsNullOrWhiteSpace(downpayment))
                finalNotes = (finalNotes + " | Downpayment: " + downpayment).Trim();

            var booking = new tbl_bookings
            {
                CustomerId = customerId,
                ServiceId = serviceId,
                BookingDate = date.Date,
                StartTime = st,
                EndTime = et,
                Status = "Pending",
                Notes = finalNotes,
                CreatedAt = DateTime.Now
            };

            db.tbl_bookings.Add(booking);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Booking created.",
                bookingId = booking.BookingId
            });
        }

        public ActionResult List()
        {
            var items = db.tbl_bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(50)
                .Select(b => new
                {
                    b.BookingId,
                    b.CustomerId,
                    b.ServiceId,
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime.ToString(),
                    EndTime = b.EndTime.ToString(),
                    b.Status,
                    b.Notes,
                    b.CreatedAt
                })
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }


        // POST: /Booking/UpdateStatus
        // body: bookingId=1&status=Approved
        [HttpPost]
        public ActionResult UpdateStatus(int bookingId, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Json(new { success = false, message = "Status is required." });

            // allow only these statuses (edit if your team wants other names)
            var allowed = new[] { "Pending", "Approved", "Completed", "Cancelled" };
            if (!allowed.Contains(status))
                return Json(new { success = false, message = "Invalid status." });

            var booking = db.tbl_bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            booking.Status = status;
            db.SaveChanges();

            return Json(new { success = true, message = $"Booking #{bookingId} updated to {status}." });
        }

        // POST: /Booking/Cancel
        // body: bookingId=1&reason=customer%20not%20available
        [HttpPost]
        public ActionResult Cancel(int bookingId, string reason = null)
        {
            var booking = db.tbl_bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            booking.Status = "Cancelled";

            if (!string.IsNullOrWhiteSpace(reason))
            {
                var note = $"Cancel reason: {reason}";
                if (string.IsNullOrWhiteSpace(booking.Notes))
                    booking.Notes = note;
                else if (!booking.Notes.Contains("Cancel reason:"))
                    booking.Notes = booking.Notes + " | " + note;
            }

            db.SaveChanges();

            return Json(new { success = true, message = $"Booking #{bookingId} cancelled." });
        }



        public ActionResult TestDb()
        {
            var count = db.tbl_bookings.Count();
            return Content("DB OK. Booking rows = " + count);
        }

        private bool TryParseTime(string s, out TimeSpan t)
        {
            t = default(TimeSpan);

            if (string.IsNullOrWhiteSpace(s)) return false;

            if (TimeSpan.TryParseExact(s, @"hh\:mm", CultureInfo.InvariantCulture, out t)) return true;
            if (TimeSpan.TryParseExact(s, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out t)) return true;

            return TimeSpan.TryParse(s, out t);
        }
    }
}
