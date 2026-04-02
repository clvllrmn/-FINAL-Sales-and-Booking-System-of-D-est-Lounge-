using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
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
            // ✅ LOGIN CHECK (OLD)
            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Please login first." });

            if (customerId <= 0)
                return Json(new { success = false, message = "User not logged in." });

            var userExists = db.tbl_users.Any(u => u.userID == customerId);
            if (!userExists)
                return Json(new { success = false, message = "User does not exist." });

            // ✅ DATE PARSE
            DateTime date;
            var dateFormats = new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };

            if (!DateTime.TryParseExact(bookingDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out date))
            {
                if (!DateTime.TryParse(bookingDate, out date))
                    return Json(new { success = false, message = "Invalid bookingDate." });
            }

            // ✅ TIME PARSE
            if (!TryParseTime(startTime, out TimeSpan st))
                return Json(new { success = false, message = "Invalid startTime." });

            if (!TryParseTime(endTime, out TimeSpan et))
                return Json(new { success = false, message = "Invalid endTime." });

            if (et <= st)
                return Json(new { success = false, message = "EndTime must be after StartTime." });

            // 🔥 24-HOUR RULE (RESTORED)
            var bookingDateTime = date.Date + st;
            if (bookingDateTime < DateTime.Now.AddHours(24))
            {
                return Json(new
                {
                    success = false,
                    message = "Booking must be at least 24 hours in advance."
                });
            }

            // 🔥 DOWNPAYMENT REQUIRED (RESTORED)
            if (string.IsNullOrWhiteSpace(downpayment))
            {
                return Json(new
                {
                    success = false,
                    message = "Downpayment is required."
                });
            }

            // 🔥 CONFLICT CHECK WITH NAIL TECH (MERGED)
            var conflict = db.tbl_bookings.Any(b =>
                b.BookingDate == date.Date &&
                (b.Status == "Pending" || b.Status == "Approved") &&
                (
                    string.IsNullOrEmpty(nailTech) ||
                    b.Notes.Contains("NailTech: " + nailTech)
                ) &&
                !(et <= b.StartTime || st >= b.EndTime)
            );

            if (conflict)
                return Json(new { success = false, message = "Time slot already taken." });

            // ✅ NOTES BUILDING
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

            // ✅ 24-HOUR CHECK FOR APPROVALS
            if (status == "Approved")
            {
                DateTime selectedDateTime = booking.BookingDate.Date + booking.StartTime;
                DateTime now = DateTime.Now;

                if (selectedDateTime < now.AddHours(24))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot approve bookings that are less than 24 hours away."
                    });
                }
            }

            // ✅ TIME LOCK FIX (BEST VERSION)
            if (status == "Completed")
            {
                var bookingEndDateTime = booking.BookingDate.Date + booking.EndTime;

                if (bookingEndDateTime > DateTime.Now)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot complete future appointment."
                    });
                }
            }

            booking.Status = status;
            db.SaveChanges();

            // ✅ EMAIL (NEW)
            if (status == "Approved")
            {
                SendBookingEmail(booking);
                CreateNotification(
                    booking.CustomerId,
                    $"Your Booking #{booking.BookingId} has been APPROVED! ✅"
                );
            }

            // ✅ NOTIFICATION FOR CANCELLED
            if (status == "Cancelled")
            {
                CreateNotification(
                    booking.CustomerId,
                    $"Your booking #{booking.BookingId} has been CANCELLED ❌"
                );
            }

            // ✅ NOTIFICATION + REVIEW FOR COMPLETED
            if (status == "Completed")
            {
                CreateNotification(
                    booking.CustomerId,
                    $"Your booking #{booking.BookingId} is completed! Please leave a review ⭐"
                );

                CreateReviewRequest(booking.CustomerId, booking.BookingId);
            }

            return Json(new
            {
                success = true,
                message = $"Booking #{bookingId} updated to {status}."
            });
        }

        // REPLACE your existing Cancel action in BookingController.cs with this
        [HttpPost]
        public ActionResult Cancel(int bookingId, string reason = null)
        {
            var booking = db.tbl_bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            // ── Only Pending or Approved bookings can be cancelled ──
            if (booking.Status != "Pending" && booking.Status != "Approved")
                return Json(new { success = false, message = "Only Pending or Approved bookings can be cancelled." });

            // ── 24-hour rule: cannot cancel within 24 hours of booking ──
            DateTime bookingDateTime = booking.BookingDate.Date + booking.StartTime;
            if (bookingDateTime <= DateTime.Now.AddHours(24))
            {
                return Json(new
                {
                    success = false,
                    message = "Bookings cannot be cancelled within 24 hours of the appointment."
                });
            }

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

            // ── Notify the customer ──
            CreateNotification(
                booking.CustomerId,
                $"Your booking #{booking.BookingId} has been CANCELLED ❌"
            );

            return Json(new { success = true, message = $"Booking #{bookingId} cancelled successfully." });
        }

        [HttpPost]
        public ActionResult CreateWithReceipt()
        {
            var file = Request.Files["receipt"];

            string filePath = null;

            var uploadsFolder = Server.MapPath("~/Uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(uploadsFolder, fileName);

            file.SaveAs(path);

            filePath = "/Uploads/" + fileName;

            int customerId, serviceId;
            DateTime bookingDate;
            TimeSpan startTime, endTime;

            // VALIDATION
            if (!int.TryParse(Request["customerId"], out customerId))
            {
                return Json(new { success = false, message = "Invalid customerId" });
            }

            if (!int.TryParse(Request["serviceId"], out serviceId))
            {
                return Json(new { success = false, message = "Invalid serviceId" });
            }

            if (!DateTime.TryParse(Request["bookingDate"], out bookingDate))
            {
                return Json(new { success = false, message = "Invalid bookingDate" });
            }

            if (!TimeSpan.TryParse(Request["startTime"], out startTime))
            {
                return Json(new { success = false, message = "Invalid startTime" });
            }

            if (!TimeSpan.TryParse(Request["endTime"], out endTime))
            {
                return Json(new { success = false, message = "Invalid endTime" });
            }

            // ✅ ADD 24-HOUR ADVANCE VALIDATION HERE
            DateTime selectedDateTime = bookingDate.Date + startTime;
            DateTime now = DateTime.Now;

            if (selectedDateTime < now.AddHours(24))
            {
                return Json(new
                {
                    success = false,
                    message = "Bookings must be at least 24 hours in advance."
                });
            }

            var services = Request["services"] ?? "";
            var nailTech = Request["nailTech"] ?? "";
            var downpayment = Request["downpayment"] ?? "";

            // Updated Notes construction
            var notes = "Services: " + services +
                        " | NailTech: " + nailTech +
                        " | Downpayment: " + downpayment +
                        " | Receipt: " + filePath;

            var booking = new tbl_bookings
            {
                CustomerId = customerId,
                ServiceId = serviceId,
                BookingDate = bookingDate,
                StartTime = startTime,
                EndTime = endTime,
                NailTech = Request["nailTech"],
                Status = "Pending",
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            var conflict = db.tbl_bookings.Any(b =>
                b.BookingDate == bookingDate.Date &&
                (b.Status == "Pending" || b.Status == "Approved") &&
                b.Notes.Contains(nailTech) &&
                !(endTime <= b.StartTime || startTime >= b.EndTime)
            );

            if (conflict)
            {
                return Json(new
                {
                    success = false,
                    message = "This time slot is already taken."
                });
            }

            var existing = db.tbl_bookings.ToList().Any(b =>
            {
                var sameDate = b.BookingDate.Date == booking.BookingDate.Date;
                var sameNailTech = (b.NailTech ?? "") == (booking.NailTech ?? "");
                var notCancelled = b.Status != "Cancelled";

                var overlap =
                    (booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
                    (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
                    (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime);

                return sameDate && sameNailTech && notCancelled && overlap;
            });

            db.tbl_bookings.Add(booking);
            db.SaveChanges();

            return Json(new { success = true });
        }

        public string NailTech { get; set; }

        private void CreateNotification(int customerId, string message)
        {
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

        public ActionResult TestDb()
        {
            var count = db.tbl_bookings.Count();
            return Content("DB OK. Booking rows = " + count);
        }

        [HttpGet]
        public JsonResult CheckSlot(string date, string startTime, string nailTech)
        {
            try
            {
                // Parse the date
                DateTime bookingDate;
                var dateFormats = new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };

                if (!DateTime.TryParseExact(date, dateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out bookingDate))
                {
                    if (!DateTime.TryParse(date, out bookingDate))
                    {
                        return Json(new { taken = false, error = "Invalid date format" }, JsonRequestBehavior.AllowGet);
                    }
                }

                // Parse start time
                if (!TryParseTime(startTime, out TimeSpan start))
                {
                    return Json(new { taken = false, error = "Invalid time format" }, JsonRequestBehavior.AllowGet);
                }

                // Calculate end time (2 hours after start)
                var end = start.Add(TimeSpan.FromHours(2));

                // Check for conflicts
                var exists = db.tbl_bookings.Any(b =>
                    b.BookingDate.Date == bookingDate.Date &&
                    (b.NailTech ?? "") == (nailTech ?? "") &&
                    b.Status != "Cancelled" &&
                    (
                        (start >= b.StartTime && start < b.EndTime) ||
                        (end > b.StartTime && end <= b.EndTime) ||
                        (start <= b.StartTime && end >= b.EndTime)
                    )
                );

                return Json(new { taken = exists }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CheckSlot Error: " + ex.Message);
                return Json(new { taken = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool TryParseTime(string s, out TimeSpan t)
        {
            t = default(TimeSpan);

            if (string.IsNullOrWhiteSpace(s)) return false;

            if (TimeSpan.TryParseExact(s, @"hh\:mm", CultureInfo.InvariantCulture, out t)) return true;
            if (TimeSpan.TryParseExact(s, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out t)) return true;

            return TimeSpan.TryParse(s, out t);
        }

        private void SendBookingEmail(tbl_bookings booking)
        {
            try
            {
                var user = db.tbl_users.FirstOrDefault(u => u.userID == booking.CustomerId);
                if (user == null || string.IsNullOrEmpty(user.email)) return;

                string subject = "D'est Lounge Booking Confirmation";

                string body = $@"
                Hello {user.firstname},

                Your booking has been APPROVED!

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

                mail.To.Add(user.email);
                client.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EMAIL ERROR: " + ex.Message);
            }

        }

        public JsonResult GetTakenSlots(DateTime date)
        {
            var slots = db.tbl_bookings
                .Where(b => b.BookingDate == date && b.Status != "Cancelled")
                .Select(b => b.StartTime)
                .ToList();

            return Json(slots, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInboxItems()
        {
            var items = db.tbl_bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(50)
                .ToList()
                .Select(b => new
                {
                    bookingId = b.BookingId,
                    clientName = db.tbl_users
                        .Where(u => u.userID == b.CustomerId)
                        .Select(u => u.firstname + " " + u.lastname)
                        .FirstOrDefault() ?? ("Customer #" + b.CustomerId),
                    service = b.Notes != null && b.Notes.Contains("Services:")
                        ? b.Notes.Split('|')[0].Replace("Services:", "").Trim()
                        : "N/A",
                    bookingDate = b.BookingDate,
                    startTime = b.StartTime.ToString(),
                    endTime = b.EndTime.ToString(),
                    status = b.Status,
                    notes = b.Notes,
                    createdAt = b.CreatedAt
                })
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

    }
}