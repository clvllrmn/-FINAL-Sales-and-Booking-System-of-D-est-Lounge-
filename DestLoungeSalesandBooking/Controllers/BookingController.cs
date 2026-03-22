using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Configuration;

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
            if (customerId <= 0)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var userExists = db.tbl_users.Any(u => u.userID == customerId);
            if (!userExists)
            {
                return Json(new { success = false, message = "User does not exist." });
            }

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

        private void CreateNotification(int customerId, string message)
        {
            // ⚠️ Make sure you have tbl_notifications table
            var notif = new tbl_notifications
            {
                CustomerId = customerId,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            db.tbl_notifications.Add(notif);
            db.SaveChanges();
        }


        private void CreateReviewRequest(int customerId, int bookingId)

        {
            var review = new tbl_review_requests
            {
                CustomerId = customerId,
                BookingId = bookingId,
                IsReviewed = false,
                CreatedAt = DateTime.Now
            };

            db.tbl_review_requests.Add(review);
            db.SaveChanges();
        }

        public ActionResult List()
        {
            var items = db.tbl_bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(50)
                .ToList()
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
                    b.CreatedAt,
                    FirstName = db.tbl_users
                        .Where(u => u.userID == b.CustomerId)
                        .Select(u => u.firstname)
                        .FirstOrDefault(),
                    LastName = db.tbl_users
                        .Where(u => u.userID == b.CustomerId)
                        .Select(u => u.lastname)
                        .FirstOrDefault()
                })
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int bookingId, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Json(new { success = false, message = "Status is required." });

            var allowed = new[] { "Pending", "Approved", "Completed", "Cancelled" };
            if (!allowed.Contains(status))
                return Json(new { success = false, message = "Invalid status." });

            var booking = db.tbl_bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            if (status == "Completed")
            {
                DateTime bookingDateTime = booking.BookingDate.Date + booking.StartTime;

                if (bookingDateTime > DateTime.Now)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot complete a future appointment."
                    });
                }
            }

            booking.Status = status;
            db.SaveChanges();
            // SEND EMAIL WHEN APPROVED
            if (status == "Approved")
            {
                SendBookingEmail(booking);
            }

            if (status == "Completed")
            {
                CreateNotification(
                    booking.CustomerId,
                    $"Your booking #{booking.BookingId} is completed! Please leave a review."
                );

                CreateReviewRequest(booking.CustomerId, booking.BookingId);
            }

            return Json(new
            {
                success = true,
                message = $"Booking #{bookingId} updated to {status}."
            });
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

        public ActionResult GetUserBookings(int userId)
        {
            var bookings = db.tbl_bookings
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingDate,
                    StartTime = b.StartTime.ToString(),
                    EndTime = b.EndTime.ToString(),
                    b.Status,
                    b.Notes
                })
                .ToList();

            return Json(bookings, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNotifications(int userId)
        {
            var notifs = db.tbl_notifications
                .Where(n => n.CustomerId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Message,
                    n.CreatedAt,
                    n.IsRead
                })
                .ToList();

            return Json(notifs, JsonRequestBehavior.AllowGet);
        }


        private void SendBookingEmail(tbl_bookings booking)
        {
            try
            {
                var user = db.tbl_users.FirstOrDefault(u => u.userID == booking.CustomerId);
                if (user == null || string.IsNullOrEmpty(user.email)) return;

                string toEmail = user.email;

                string subject = "D'est Lounge Booking Confirmation";

                string body = $@"
Hello {user.firstname},

Your booking has been APPROVED!

Booking Details:
--------------------------
Date: {booking.BookingDate:MMMM dd, yyyy}
Time: {booking.StartTime} - {booking.EndTime}
Notes: {booking.Notes}

Thank you for choosing D'est Lounge 💅
";

                var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
                var smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                var fromName = ConfigurationManager.AppSettings["SmtpFromName"];

                var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpEmail, smtpPass),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(smtpEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mail.To.Add(toEmail);

                client.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EMAIL ERROR: " + ex.Message);
            }
        }

    }
}
