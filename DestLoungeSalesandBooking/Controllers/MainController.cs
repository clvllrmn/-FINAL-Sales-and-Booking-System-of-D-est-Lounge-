using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using DestLoungeSalesandBooking.Models.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class MainController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext(); // Replace with your actual DbContext name

        // GET: Main
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

        public ActionResult LoginPage()
        {
            return View();
        }

        // GET: SignupPage
        public ActionResult SignupPage()
        {
            return View();
        }

        // POST: SignupPage - Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignupPage(string FNAME, string LNAME, string EMAIL, string CONTACT, string ADDRESS, string PASSWORD, string CONFIRMPASSWORD)
        {
            try
            {
                // Debug: Log what we received
                System.Diagnostics.Debug.WriteLine($"Received - FNAME: {FNAME}, LNAME: {LNAME}, EMAIL: {EMAIL}, CONTACT: {CONTACT}, PASSWORD Length: {PASSWORD?.Length}");

                // Validate input
                if (string.IsNullOrWhiteSpace(FNAME) || string.IsNullOrWhiteSpace(LNAME) ||
                    string.IsNullOrWhiteSpace(EMAIL) || string.IsNullOrWhiteSpace(CONTACT) ||
                    string.IsNullOrWhiteSpace(ADDRESS) || string.IsNullOrWhiteSpace(PASSWORD))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return View();
                }

                // Check if passwords match
                if (PASSWORD != CONFIRMPASSWORD)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return View();
                }

                // Check if email already exists
                var existingUser = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == EMAIL.ToLower().Trim());
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email already registered. Please use a different email or login.";
                    return View();
                }

                // Validate contact number format (must be exactly 11 digits)
                if (!System.Text.RegularExpressions.Regex.IsMatch(CONTACT, @"^\d{11}$"))
                {
                    TempData["ErrorMessage"] = "Contact number must be exactly 11 digits.";
                    return View();
                }


                // Validate password strength - allow more special characters
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

                // Hash the password for security
                string hashedPassword = HashPassword(PASSWORD);

                // Create new user object
                var newUser = new tbl_users
                {
                    roleID = 2, // 2 = Customer role (adjust if needed)
                    firstname = FNAME.Trim(),
                    lastname = LNAME.Trim(),
                    email = EMAIL.Trim().ToLower(),
                    password = hashedPassword,
                    coNum = CONTACT,
                    address = ADDRESS.Trim(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };

                // Add to database and save
                db.tbl_users.Add(newUser);
                db.SaveChanges();

                // Success - redirect to login
                TempData["SuccessMessage"] = "Registration successful! Please login with your new account.";
                return RedirectToAction("LoginPage", "Main");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Handle validation errors
                string errorMessages = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages += validationError.PropertyName + ": " + validationError.ErrorMessage + "\n";
                    }
                }
                TempData["ErrorMessage"] = "Validation error: " + errorMessages;
                return View();
            }
            catch (Exception ex)
            {
                // This looks for the "real" error hidden inside Entity Framework
                var realError = ex.InnerException?.InnerException?.Message ?? ex.Message;

                System.Diagnostics.Debug.WriteLine("Signup Error: " + ex.ToString());
                TempData["ErrorMessage"] = "Technical Error: " + realError;
                return View();
            }
        }

        public ActionResult BookingPage()
        {
            return View();
        }

        public ActionResult ProfilePage()
        {
            return View();
        }

        // Helper method to hash passwords using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert password string to bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Clean up database connection when controller is disposed
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