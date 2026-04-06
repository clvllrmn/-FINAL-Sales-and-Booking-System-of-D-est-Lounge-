using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace DestLoungeSalesandBooking.Controllers
{

    public class AccountController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // ─────────────────────────────────────────────────────────────────────
        // Lockout tracking (in-memory, shared across all requests)
        // ─────────────────────────────────────────────────────────────────────
        private static readonly Dictionary<string, LoginAttemptInfo> _loginAttempts
            = new Dictionary<string, LoginAttemptInfo>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _lockObj = new object();

        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

        private class LoginAttemptInfo
        {
            public int FailedCount { get; set; }
            public DateTime? LockoutExpiry { get; set; }
        }
        // ─────────────────────────────────────────────────────────────────────

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    TempData["ErrorMessage"] = "Please enter both email and password.";
                    return RedirectToAction("LoginPage", "Main");
                }

                string emailKey = email.Trim().ToLower();

                // ── Step 1: Check if account is currently locked out ──────────
                lock (_lockObj)
                {
                    if (_loginAttempts.TryGetValue(emailKey, out var attemptInfo))
                    {
                        if (attemptInfo.LockoutExpiry.HasValue && attemptInfo.LockoutExpiry.Value > DateTime.Now)
                        {
                            double remaining = (attemptInfo.LockoutExpiry.Value - DateTime.Now).TotalSeconds;
                            int minutes = (int)(remaining / 60);
                            int seconds = (int)(remaining % 60);

                            string timeMsg = minutes > 0
                                ? $"{minutes}m {seconds}s"
                                : $"{seconds}s";

                            TempData["ErrorMessage"] =
                                $"Account temporarily locked due to too many failed attempts. " +
                                $"Please try again in {timeMsg}.";
                            return RedirectToAction("LoginPage", "Main");
                        }
                    }
                }
                // ─────────────────────────────────────────────────────────────

                // ── Step 2: Validate credentials ──────────────────────────────
                // ── Step 2: Validate credentials ──────────────────────────────
                var user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == emailKey);
                string hashedPassword = HashPassword(password);
                bool credentialsValid = user != null && user.password == hashedPassword;
                // ─────────────────────────────────────────────────────────────
                // ─────────────────────────────────────────────────────────────

                if (!credentialsValid)
                {
                    // ── Step 3: Record the failed attempt ─────────────────────
                    lock (_lockObj)
                    {
                        if (!_loginAttempts.ContainsKey(emailKey))
                            _loginAttempts[emailKey] = new LoginAttemptInfo();

                        var info = _loginAttempts[emailKey];
                        info.FailedCount++;

                        if (info.FailedCount >= MaxFailedAttempts)
                        {
                            info.LockoutExpiry = DateTime.Now.Add(LockoutDuration);
                            TempData["ErrorMessage"] =
                                "Too many failed login attempts. Your account has been locked for 5 minutes.";
                        }
                        else
                        {
                            int attemptsLeft = MaxFailedAttempts - info.FailedCount;
                            TempData["ErrorMessage"] =
                                $"Invalid email or password. " +
                                $"{attemptsLeft} attempt{(attemptsLeft == 1 ? "" : "s")} remaining before temporary lockout.";
                        }
                    }
                    // ─────────────────────────────────────────────────────────

                    return RedirectToAction("LoginPage", "Main");
                }

                // ── Step 4: Successful login — clear failed attempts ──────────
                lock (_lockObj)
                {
                    if (_loginAttempts.ContainsKey(emailKey))
                        _loginAttempts.Remove(emailKey);
                }
                // ─────────────────────────────────────────────────────────────

                FormsAuthentication.SetAuthCookie(user.email, false);

                Session["UserID"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserFirstName"] = user.firstname;
                Session["UserLastName"] = user.lastname;
                Session["RoleID"] = user.roleID;
                Session["FullName"] = user.firstname + " " + user.lastname;

                if (user.roleID == 1)
                    return RedirectToAction("AdminHomepage", "Main");
                else
                    return RedirectToAction("Homepage", "Main");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                System.Diagnostics.Debug.WriteLine("Login Error: " + ex.ToString());
                return RedirectToAction("LoginPage", "Main");
            }
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            // ← These must come BEFORE the return
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetAllowResponseInBrowserHistory(false);

            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToAction("LoginPage", "Main");
        }

        // GET: Account/ForgotPassword - Redirects to the page
        public ActionResult ForgotPassword()
        {
            return RedirectToAction("ForgotPasswordPage", "Main");
        }

        // ─────────────────────────────────────────────
        // POST: Account/ResetPassword (For regular users)
        // Handles password reset from ForgotPasswordPage
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);
                bool hasSpecial = newPassword.Any(c => "@$!%*?&#".Contains(c));
                bool isLong = newPassword.Length >= 8;

                if (!hasLower || !hasUpper || !hasDigit || !hasSpecial || !isLong)
                {
                    string details = "";
                    if (!isLong) details += "At least 8 characters. ";
                    if (!hasUpper) details += "One uppercase letter. ";
                    if (!hasLower) details += "One lowercase letter. ";
                    if (!hasDigit) details += "One number. ";
                    if (!hasSpecial) details += "One special character (@$!%*?&#). ";

                    TempData["ErrorMessage"] = "Password requirements not met: " + details;
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                var user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == email.ToLower().Trim());

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Email not found.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                string hashedNewPassword = HashPassword(newPassword);

                if (user.password == hashedNewPassword)
                {
                    TempData["ErrorMessage"] = "New password cannot be the same as your current password.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                user.password = hashedNewPassword;
                user.updatedAt = DateTime.Now;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
                return RedirectToAction("LoginPage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ResetPassword Error: " + ex.ToString());
                TempData["ErrorMessage"] = "A technical error occurred. Please try again.";
                return RedirectToAction("ForgotPasswordPage", "Main");
            }
        }

        // ─────────────────────────────────────────────
        // GET: Account/ChangePassword  (Admin only)
        // ─────────────────────────────────────────────
        public ActionResult ChangePassword()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("LoginPage", "Main");
            }
            return RedirectToAction("AdminChangePasswordPage", "Main");
        }

        // ─────────────────────────────────────────────
        // POST: Account/ChangePassword  (Admin only)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("LoginPage", "Main");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(currentPassword) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmNewPassword))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return RedirectToAction("AdminChangePasswordPage", "Main");
                }

                if (newPassword != confirmNewPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToAction("AdminChangePasswordPage", "Main");
                }

                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);
                bool hasSpecial = newPassword.Any(c => "@$!%*?&#".Contains(c));
                bool isLong = newPassword.Length >= 8;

                if (!hasLower || !hasUpper || !hasDigit || !hasSpecial || !isLong)
                {
                    string details = "";
                    if (!isLong) details += "At least 8 characters. ";
                    if (!hasUpper) details += "One uppercase letter. ";
                    if (!hasLower) details += "One lowercase letter. ";
                    if (!hasDigit) details += "One number. ";
                    if (!hasSpecial) details += "One special character (@$!%*?&#). ";

                    TempData["ErrorMessage"] = "Password requirements not met: " + details;
                    return RedirectToAction("AdminChangePasswordPage", "Main");
                }

                var admin = db.tbl_users.FirstOrDefault(u =>
                    u.email.ToLower() == "deestlounge@gmail.com" && u.roleID == 1);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "Admin account not found.";
                    return RedirectToAction("AdminChangePasswordPage", "Main");
                }

                if (admin.password != HashPassword(currentPassword))
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return RedirectToAction("AdminChangePasswordPage", "Main");
                }

                admin.password = HashPassword(newPassword);
                admin.updatedAt = DateTime.Now;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToAction("AdminHomepage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ChangePassword Error: " + ex.ToString());
                TempData["ErrorMessage"] = "A technical error occurred. Please try again.";
                return RedirectToAction("AdminChangePasswordPage", "Main");
            }
        }

        // ─────────────────────────────────────────────
        // Helper: SHA256 password hashing
        // ─────────────────────────────────────────────
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
        //Google Sign In
        [HttpPost]
        public async Task<ActionResult> GoogleSignIn()
        {
            try
            {
                Request.InputStream.Position = 0;
                string body;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                var request = serializer.Deserialize<DestLoungeSalesandBooking.Models.GoogleSignInRequest>(body);

                if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                {
                    return Json(new { success = false, message = "Missing Google token." });
                }

                var clientId = ConfigurationManager.AppSettings["GoogleClientId"];

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                if (payload == null)
                {
                    return Json(new { success = false, message = "Invalid Google token." });
                }

                var user = db.tbl_users.FirstOrDefault(u => u.googleSub == payload.Subject);

                if (user == null && !string.IsNullOrWhiteSpace(payload.Email))
                {
                    user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == payload.Email.ToLower());
                }

                if (user == null)
                {
                    user = new tbl_users
                    {
                        roleID = 2,
                        firstname = payload.GivenName ?? "Google",
                        lastname = payload.FamilyName ?? "User",
                        email = payload.Email ?? "",
                        password = "GOOGLE_LOGIN_ONLY",
                        coNum = "",
                        address = "",
                        googleSub = payload.Subject,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    };

                    db.tbl_users.Add(user);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user.googleSub))
                        user.googleSub = payload.Subject;

                    if (string.IsNullOrWhiteSpace(user.firstname) && !string.IsNullOrWhiteSpace(payload.GivenName))
                        user.firstname = payload.GivenName;

                    if (string.IsNullOrWhiteSpace(user.lastname) && !string.IsNullOrWhiteSpace(payload.FamilyName))
                        user.lastname = payload.FamilyName;

                    if (string.IsNullOrWhiteSpace(user.email) && !string.IsNullOrWhiteSpace(payload.Email))
                        user.email = payload.Email;

                    user.updatedAt = DateTime.Now;
                }

                db.SaveChanges();

                FormsAuthentication.SetAuthCookie(user.email, false);

                Session["UserID"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserFirstName"] = user.firstname;
                Session["UserLastName"] = user.lastname;
                Session["RoleID"] = user.roleID;
                Session["FullName"] = user.firstname + " " + user.lastname;

                string redirectUrl = user.roleID == 1
                    ? Url.Action("AdminHomepage", "Main")
                    : Url.Action("Homepage", "Main");

                return Json(new
                {
                    success = true,
                    redirectUrl = redirectUrl
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GoogleSignIn Error: " + ex.ToString());
                return Json(new { success = false, message = "Google sign-in failed." });
            }
        
    }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendForgotPasswordOtp(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Please enter your email.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                email = email.Trim();

                var user = db.tbl_users.FirstOrDefault(u => u.email == email);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Email not found.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                var otp = new Random().Next(100000, 999999).ToString();

                Session["ForgotPasswordOTP"] = otp;
                Session["ForgotPasswordEmail"] = email;
                Session["ForgotPasswordOTPExpiry"] = DateTime.Now.AddMinutes(5); // ✅ FIXED

                // SEND EMAIL
                SendOtpEmail(user.email, user.firstname, otp);

                TempData["SuccessMessage"] = "OTP has been sent to your email.";
                TempData["ShowOtpStep"] = true;
                TempData["ResetEmail"] = email;

                return RedirectToAction("ForgotPasswordPage", "Main");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ForgotPasswordPage", "Main");
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOtpAndResetPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(otp) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                if (Session["ForgotPasswordOTP"] == null ||
                    Session["ForgotPasswordEmail"] == null ||
                    Session["ForgotPasswordOTPExpiry"] == null)
                {
                    TempData["ErrorMessage"] = "OTP session expired. Please request a new OTP.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                var savedOtp = Session["ForgotPasswordOTP"].ToString();
                var savedEmail = Session["ForgotPasswordEmail"].ToString();
                var expiry = (DateTime)Session["ForgotPasswordOTPExpiry"];

                if (DateTime.Now > expiry)
                {
                    Session.Remove("ForgotPasswordOTP");
                    Session.Remove("ForgotPasswordEmail");
                    Session.Remove("ForgotPasswordOTPExpiry");

                    TempData["ErrorMessage"] = "OTP expired. Please request a new one.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                if (savedEmail.ToLower() != email.ToLower().Trim() || savedOtp != otp.Trim())
                {
                    TempData["ErrorMessage"] = "Invalid OTP.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                bool hasLower = newPassword.Any(char.IsLower);
                bool hasUpper = newPassword.Any(char.IsUpper);
                bool hasDigit = newPassword.Any(char.IsDigit);
                bool hasSpecial = newPassword.Any(c => "@$!%*?&#".Contains(c));
                bool isLong = newPassword.Length >= 8;

                if (!hasLower || !hasUpper || !hasDigit || !hasSpecial || !isLong)
                {
                    TempData["ErrorMessage"] = "Password must be at least 8 characters and include uppercase, lowercase, number, and symbol.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                var user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == email.ToLower().Trim());

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                user.password = HashPassword(newPassword);
                user.updatedAt = DateTime.Now;
                db.SaveChanges();

                Session.Remove("ForgotPasswordOTP");
                Session.Remove("ForgotPasswordEmail");
                Session.Remove("ForgotPasswordOTPExpiry");

                TempData["SuccessMessage"] = "Password reset successful. You may now log in.";
                return RedirectToAction("LoginPage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VerifyOtpAndResetPassword Error: " + ex.ToString());
                TempData["ErrorMessage"] = "A technical error occurred.";
                return RedirectToAction("ForgotPasswordPage", "Main");
            }
        }

        private void SendOtpEmail(string toEmail, string firstName, string otp)
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
            var fromName = ConfigurationManager.AppSettings["SmtpFromName"];

            var subject = "D'est Lounge Password Reset OTP";
            var body = $@"Hello {firstName},

Your OTP for password reset is: {otp}

This OTP is valid for 5 minutes.

If you did not request this, please ignore this email.

- D'est Lounge";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(smtpEmail, fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpEmail, smtpPass);
                    client.EnableSsl = true;
                    client.Send(message);
                }

            }

        }
    }
   
}