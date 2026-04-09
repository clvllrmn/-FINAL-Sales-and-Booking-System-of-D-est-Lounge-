using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using DestLoungeSalesandBooking.Models.Maps;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class MainController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // ── Public pages (no login required) ──
        public ActionResult Homepage()
        {
            return View();
        }

        public ActionResult ContactPage()
        {
            return View();
        }

        public ActionResult FAQsPage()
        {
            return View();
        }

        public ActionResult ServicePage()
        {
            return View();
        }




        [NoCache]
        public ActionResult LoginPage()
        {
            // If already logged in, redirect away from login page
            if (Session["UserID"] != null)
            {
                if (Session["RoleID"] != null && (int)Session["RoleID"] == 1)
                    return RedirectToAction("AdminHomepage", "Main");
                return RedirectToAction("Homepage", "Main");
            }
            return View();
        }

        [NoCache]
        public ActionResult SignupPage()
        {
            return View();
        }

        public ActionResult ForgotPasswordPage()
        {
            return View();
        }

        public ActionResult GalleryPage()
        {
            return View();
        }

        // ── User-only protected pages ──
        [SessionCheck]
        [NoCache]
        public ActionResult BookingPage()
        {
            return View();
        }

        [SessionCheck]
        [NoCache]
        public ActionResult PaymentPage()
        {
            return View();
        }

        [SessionCheck]
        [NoCache]
        public ActionResult CurrentBookingPage()
        {
            return View();
        }
        public ActionResult AdminNailTechPage()
        {
            return View();
        }

        [SessionCheck]
        [NoCache]
        public ActionResult ReviewPage(int? bookingId)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("LoginPage", "Main");

            int userId = (int)Session["UserID"];

            // If no bookingId is provided, just open the My Reviews page
            if (!bookingId.HasValue || bookingId.Value <= 0)
            {
                ViewBag.BookingId = null;
                return View();
            }

            var booking = db.tbl_bookings.FirstOrDefault(b =>
                b.BookingId == bookingId.Value &&
                b.CustomerId == userId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            if (booking.Status != "Completed")
            {
                TempData["ErrorMessage"] = "You can only review completed bookings.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            var alreadyReviewed = db.tbl_reviews.Any(r => r.BookingId == booking.BookingId && r.CustomerId == userId);

            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "You already submitted a review for this booking.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            ViewBag.BookingId = booking.BookingId;
            return View();
        }

        [SessionCheck]
        [NoCache]
        public ActionResult ProfilePage()
        {
            int userId = (int)Session["UserID"];
            var user = db.tbl_users.FirstOrDefault(u => u.userID == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("LoginPage", "Main");
            }

            ViewBag.UserName = user.firstname + " " + user.lastname;
            ViewBag.UserEmail = user.email;
            ViewBag.ContactNumber = user.coNum;
            ViewBag.Address = user.address;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionCheck]
        [NoCache]
        public ActionResult UpdateProfile(string coNum, string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(coNum) || string.IsNullOrWhiteSpace(address))
                {
                    TempData["ErrorMessage"] = "Contact number and address are required.";
                    return RedirectToAction("ProfilePage", "Main");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(coNum.Trim(), @"^\d{11}$"))
                {
                    TempData["ErrorMessage"] = "Contact number must be exactly 11 digits.";
                    return RedirectToAction("ProfilePage", "Main");
                }

                int userId = (int)Session["UserID"];
                var user = db.tbl_users.FirstOrDefault(u => u.userID == userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("ProfilePage", "Main");
                }

                user.coNum = coNum.Trim();
                user.address = address.Trim();
                user.updatedAt = DateTime.Now;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("ProfilePage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateProfile Error: " + ex.ToString());
                TempData["ErrorMessage"] = "A technical error occurred. Please try again.";
                return RedirectToAction("ProfilePage", "Main");
            }
        }

        // ── Admin-only protected pages ──
        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminHomepage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminContactPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminFAQsPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminBookingPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminServicePage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminInboxPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminSalesPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminGalleryPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminPaymentSetting()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminChangePasswordPage()
        {
            return View();
        }

        [SessionCheck(RequireAdmin = true)]
        [NoCache]
        public ActionResult AdminHomepageEditPage()
        {
            return View();
        }

        // ── POST: SignupPage ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignupPage(string FNAME, string LNAME, string EMAIL, string CONTACT, string ADDRESS, string PASSWORD, string CONFIRMPASSWORD)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Received - FNAME: {FNAME}, LNAME: {LNAME}, EMAIL: {EMAIL}, CONTACT: {CONTACT}, PASSWORD Length: {PASSWORD?.Length}");

                if (string.IsNullOrWhiteSpace(FNAME) || string.IsNullOrWhiteSpace(LNAME) ||
                    string.IsNullOrWhiteSpace(EMAIL) || string.IsNullOrWhiteSpace(CONTACT) ||
                    string.IsNullOrWhiteSpace(ADDRESS) || string.IsNullOrWhiteSpace(PASSWORD))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return View();
                }

                if (PASSWORD != CONFIRMPASSWORD)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return View();
                }

                var existingUser = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == EMAIL.ToLower().Trim());
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email already registered. Please use a different email or login.";
                    return View();
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(CONTACT, @"^\d{11}$"))
                {
                    TempData["ErrorMessage"] = "Contact number must be exactly 11 digits.";
                    return View();
                }

                bool hasLower = PASSWORD.Any(char.IsLower);
                bool hasUpper = PASSWORD.Any(char.IsUpper);
                bool hasDigit = PASSWORD.Any(char.IsDigit);
                bool hasSpecial = PASSWORD.Any(c => "@$!%*?&#".Contains(c));
                bool isLongEnough = PASSWORD.Length >= 8;

                if (!hasLower || !hasUpper || !hasDigit || !hasSpecial || !isLongEnough)
                {
                    string errorDetails = "";
                    if (!isLongEnough) errorDetails += "Must be at least 8 characters. ";
                    if (!hasLower) errorDetails += "Must contain lowercase letter. ";
                    if (!hasUpper) errorDetails += "Must contain uppercase letter. ";
                    if (!hasDigit) errorDetails += "Must contain number. ";
                    if (!hasSpecial) errorDetails += "Must contain special character (@$!%*?&#). ";

                    TempData["ErrorMessage"] = "Password requirements not met: " + errorDetails;
                    return View();
                }

                string hashedPassword = HashPassword(PASSWORD);

                var newUser = new tbl_users
                {
                    roleID = 2,
                    firstname = FNAME.Trim(),
                    lastname = LNAME.Trim(),
                    email = EMAIL.Trim().ToLower(),
                    password = hashedPassword,
                    coNum = CONTACT,
                    address = ADDRESS.Trim(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };

                db.tbl_users.Add(newUser);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Please login with your new account.";
                return RedirectToAction("LoginPage", "Main");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorMessages = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        errorMessages += validationError.PropertyName + ": " + validationError.ErrorMessage + "\n";

                TempData["ErrorMessage"] = "Validation error: " + errorMessages;
                return View();
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException?.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine("Signup Error: " + ex.ToString());
                TempData["ErrorMessage"] = "Technical Error: " + realError;
                return View();
            }
        }




        // ── Helper: SHA256 hash ──
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // Add this inside MainController.cs
        public ActionResult CheckSession()
        {
            if (Session["UserID"] == null)
                return Json(new { loggedIn = false }, JsonRequestBehavior.AllowGet);

            return Json(new { loggedIn = true }, JsonRequestBehavior.AllowGet);
        }

        // ── POST: SubmitReview ──

        // Replace SubmitReview action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionCheck]
        public ActionResult SubmitReview(int BookingId, int? Rating, string ReviewText, IEnumerable<HttpPostedFileBase> PhotoUpload)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("LoginPage", "Main");

            int userId = (int)Session["UserID"];

            // VALIDATION: Rating is REQUIRED
            if (!Rating.HasValue || Rating.Value < 1 || Rating.Value > 5)
            {
                TempData["ErrorMessage"] = "Please select a star rating (1-5 stars).";
                return RedirectToAction("ReviewPage", "Main", new { bookingId = BookingId });
            }

            var booking = db.tbl_bookings.FirstOrDefault(b =>
                b.BookingId == BookingId &&
                b.CustomerId == userId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            if (booking.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Only completed bookings can be reviewed.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            var alreadyReviewed = db.tbl_reviews.Any(r => r.BookingId == BookingId && r.CustomerId == userId);
            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "You already submitted a review for this booking.";
                return RedirectToAction("CurrentBookingPage", "Main");
            }

            var review = new tbl_reviews
            {
                BookingId = BookingId,
                CustomerId = userId,
                Rating = Rating.Value,
                ReviewText = ReviewText ?? "", // Optional - can be empty
                CreatedAt = DateTime.Now
            };

            db.tbl_reviews.Add(review);
            db.SaveChanges();

            // Handle image uploads (optional)
            if (PhotoUpload != null)
            {
                // Create directory if not exists
                string uploadDir = Server.MapPath("~/Uploads/Reviews");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                foreach (var file in PhotoUpload)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        // Validate file type
                        string extension = Path.GetExtension(file.FileName).ToLower();
                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                        {
                            string fileName = Guid.NewGuid().ToString() + extension;
                            string path = Path.Combine(uploadDir, fileName);
                            file.SaveAs(path);

                            db.tbl_review_images.Add(new tbl_review_images
                            {
                                ReviewId = review.ReviewId,
                                ImageUrl = "/Uploads/Reviews/" + fileName
                            });
                        }
                    }
                }

                db.SaveChanges();
            }

            var req = db.tbl_review_requests.FirstOrDefault(r => r.BookingId == BookingId);
            if (req != null)
            {
                req.IsReviewed = true;
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Thank you for your review!";
            return RedirectToAction("GalleryPage");
        }

        [SessionCheck]
        [HttpGet]
        public ActionResult GetMyReviews()
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Please login first." }, JsonRequestBehavior.AllowGet);

                int userId = Convert.ToInt32(Session["UserID"]);

                var reviews = db.tbl_reviews
                    .Where(r => r.CustomerId == userId && !r.IsArchived)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.BookingId,
                        r.Rating,
                        r.ReviewText,
                        r.CreatedAt,
                        r.Flagged,
                        r.FlagReason,
                        r.FlagNote,
                        Images = db.tbl_review_images
                            .Where(img => img.ReviewId == r.ReviewId)
                            .Select(img => img.ImageUrl)
                            .ToList(),

                        Booking = db.tbl_bookings
                            .Where(b => b.BookingId == r.BookingId)
                            .Select(b => new
                            {
                                b.ReferenceNo,
                                BookingDate = b.BookingDate,
                                b.StartTime,
                                b.EndTime,
                                b.NailTech,
                                b.Notes
                            })
                            .FirstOrDefault()
                    })
                    .ToList();

                return Json(new { success = true, data = reviews }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [SessionCheck]
        [HttpPost]
        public ActionResult DeleteMyReview(int reviewId)
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Please login first." });

                int userId = Convert.ToInt32(Session["UserID"]);

                var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId && r.CustomerId == userId);
                if (review == null)
                    return Json(new { success = false, message = "Review not found." });

                var images = db.tbl_review_images.Where(x => x.ReviewId == reviewId).ToList();
                if (images.Any())
                    db.tbl_review_images.RemoveRange(images);

                db.tbl_reviews.Remove(review);
                db.SaveChanges();

                return Json(new { success = true, message = "Review deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [SessionCheck]
        [HttpPost]
        public ActionResult UpdateMyReview(int reviewId, int rating, string reviewText)
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Please login first." });

                int userId = Convert.ToInt32(Session["UserID"]);

                var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId && r.CustomerId == userId);
                if (review == null)
                    return Json(new { success = false, message = "Review not found." });

                review.Rating = rating;
                review.ReviewText = reviewText ?? "";
                db.SaveChanges();

                return Json(new { success = true, message = "Review updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult GetPublicReviews()
        {
            var reviews = db.tbl_reviews
                .Where(r => !r.IsArchived && !r.Flagged)
                .ToList()
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.ReviewText,
                    r.CreatedAt,
                    Flagged = r.Flagged,
                    FlagReason = r.FlagReason,
                    FlagNote = r.FlagNote,
                    IsArchived = r.IsArchived,
                    Images = db.tbl_review_images
                        .Where(img => img.ReviewId == r.ReviewId)
                        .Select(img => img.ImageUrl)
                        .ToList()
                }).ToList();

            return Json(reviews, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SessionCheck(RequireAdmin = true)]
        public ActionResult ArchiveReview(int reviewId)
        {
            var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId);

            if (review == null)
                return Json(new { success = false });

            review.IsArchived = true;
            review.ArchivedAt = DateTime.Now;

            db.SaveChanges();

            return Json(new { success = true });
        }
        [HttpGet]
        [SessionCheck(RequireAdmin = true)]
        public JsonResult GetArchivedReviews()
        {
            var reviews = db.tbl_reviews
                .Where(r => r.IsArchived)
                .ToList()
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.ReviewText,
                    r.CreatedAt,
                    Images = db.tbl_review_images
                        .Where(img => img.ReviewId == r.ReviewId)
                        .Select(img => img.ImageUrl)
                        .ToList()
                }).ToList();

            return Json(reviews, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SessionCheck(RequireAdmin = true)]
        public ActionResult RestoreReview(int reviewId)
        {
            var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId);

            if (review == null)
                return Json(new { success = false, message = "Review not found." });

            review.IsArchived = false;
            review.ArchivedAt = null; // optional

            db.SaveChanges();

            return Json(new { success = true });
        }

        // ── GET: /Main/GetNailTechs ──
        [HttpGet]
        public JsonResult GetNailTechs()
        {
            try
            {
                var techs = db.tbl_nailtech
                    .Where(t => !t.isDeleted)
                    .OrderBy(t => t.name)
                    .Select(t => new
                    {
                        nailTechId = t.nailTechId,
                        name = t.name,
                        specialization = t.specialization,
                        contact = t.contact,
                        status = t.status,
                        notes = t.notes
                    })
                    .ToList();

                return Json(techs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ── POST: /Main/AddNailTech ──
        [HttpPost]
        public JsonResult AddNailTech(tbl_nailtech model)
        {
            try
            {
                var tech = new tbl_nailtech
                {
                    name = model.name?.Trim(),
                    specialization = model.specialization?.Trim(),
                    contact = model.contact?.Trim(),
                    status = string.IsNullOrEmpty(model.status) ? "active" : model.status,
                    notes = model.notes?.Trim(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    isDeleted = false
                };

                db.tbl_nailtech.Add(tech);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST: /Main/UpdateNailTech ──
        [HttpPost]
        public JsonResult UpdateNailTech(tbl_nailtech model)
        {
            try
            {
                var tech = db.tbl_nailtech.FirstOrDefault(t => t.nailTechId == model.nailTechId && !t.isDeleted);
                if (tech == null)
                    return Json(new { success = false, message = "Nail tech not found." });

                tech.name = model.name?.Trim();
                tech.specialization = model.specialization?.Trim();
                tech.contact = model.contact?.Trim();
                tech.status = string.IsNullOrEmpty(model.status) ? tech.status : model.status;
                tech.notes = model.notes?.Trim();
                tech.updatedAt = DateTime.Now;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST: /Main/DeactivateNailTech ──
        [HttpPost]
        public JsonResult DeactivateNailTech(int nailTechId)
        {
            try
            {
                var tech = db.tbl_nailtech.FirstOrDefault(t => t.nailTechId == nailTechId && !t.isDeleted);
                if (tech == null)
                    return Json(new { success = false, message = "Nail tech not found." });

                tech.status = "inactive";
                tech.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST: /Main/ReactivateNailTech ──
        [HttpPost]
        public JsonResult ReactivateNailTech(int nailTechId)
        {
            try
            {
                var tech = db.tbl_nailtech.FirstOrDefault(t => t.nailTechId == nailTechId && !t.isDeleted);
                if (tech == null)
                    return Json(new { success = false, message = "Nail tech not found." });

                tech.status = "active";
                tech.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST: /Main/DeleteNailTech ──
        [HttpPost]
        public JsonResult DeleteNailTech(int nailTechId)
        {
            try
            {
                var tech = db.tbl_nailtech.FirstOrDefault(t => t.nailTechId == nailTechId && !t.isDeleted);
                if (tech == null)
                    return Json(new { success = false, message = "Nail tech not found." });

                tech.status = "inactive";
                tech.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST: /Main/PermanentDeleteNailTech ──
        [HttpPost]
        public JsonResult PermanentDeleteNailTech(int nailTechId)
        {
            try
            {
                var tech = db.tbl_nailtech.FirstOrDefault(t => t.nailTechId == nailTechId);
                if (tech == null)
                    return Json(new { success = false, message = "Nail tech not found." });

                db.tbl_nailtech.Remove(tech);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [SessionCheck(RequireAdmin = true)]
        public ActionResult FlagReview(int reviewId, string reason, string note)
        {
            try
            {
                var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId);

                if (review == null)
                {
                    return Json(new { success = false, message = "Review not found." });
                }

                // ✅ FLAG REVIEW
                review.Flagged = true;
                review.FlagReason = reason;
                review.FlagNote = note;
                review.FlaggedAt = DateTime.Now;

                // ✅ CREATE MESSAGE
                string message = "⚠️ Your review for booking #" + review.BookingId +
                                 " was flagged. Reason: " + reason;

                if (!string.IsNullOrWhiteSpace(note))
                {
                    message += " | Admin note: " + note;
                }

                // 🔥 VERY IMPORTANT: CHECK CustomerId
                if (review.CustomerId == 0)
                {
                    return Json(new { success = false, message = "CustomerId missing in review." });
                }

                // ✅ INSERT NOTIFICATION
                var notif = new tbl_notifications
                {
                    CustomerId = review.CustomerId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                db.tbl_notifications.Add(notif);

                // ✅ SAVE BOTH TOGETHER
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FLAG ERROR: " + ex.ToString());
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [SessionCheck(RequireAdmin = true)]
        public JsonResult GetAdminReviews()
        {
            var reviews = db.tbl_reviews
                .Where(r => !r.IsArchived) // ✅ DO NOT FILTER FLAGGED
                .ToList()
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.ReviewText,
                    r.CreatedAt,
                    r.Flagged,
                    r.FlagReason,
                    r.FlagNote,
                    Images = db.tbl_review_images
                        .Where(img => img.ReviewId == r.ReviewId)
                        .Select(img => img.ImageUrl)
                        .ToList()
                }).ToList();

            return Json(reviews, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [SessionCheck(RequireAdmin = true)]
        public ActionResult UnflagReview(int reviewId)
        {
            try
            {
                var review = db.tbl_reviews.FirstOrDefault(r => r.ReviewId == reviewId);

                if (review == null)
                {
                    return Json(new { success = false, message = "Review not found." });
                }

                review.Flagged = false;
                review.FlagReason = null;
                review.FlagNote = null;
                review.FlaggedAt = null;

                db.SaveChanges();

                return Json(new { success = true, message = "Review unflagged successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    }