using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using DestLoungeSalesandBooking.Models.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    [SessionCheck(RequireAdmin = true)]
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

        [SessionCheck]
        [NoCache]
        public ActionResult ReviewPage()
        {
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

        // ── POST: SubmitReview ──
        [HttpPost]
        public ActionResult SubmitReview(int? BookingId, int Rating, string ReviewText, IEnumerable<HttpPostedFileBase> PhotoUpload)
        {
            if (PhotoUpload != null)
            {
                foreach (var file in PhotoUpload)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string path = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                    }
                }
            }

            if (BookingId == null)
                return Content("BookingId is missing. Please access review from booking page.");

            return RedirectToAction("ReviewPage");
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
    }
}