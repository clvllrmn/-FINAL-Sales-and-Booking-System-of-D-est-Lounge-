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
using System.Web;
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
        public ActionResult Login(string email, string password, string returnUrl)
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

                System.Web.HttpContext.Current = ((System.Web.HttpContextWrapper)this.HttpContext).ApplicationInstance.Context;
                FormsAuthentication.SetAuthCookie(user.email, false);

                // Replace FormsAuthentication.SetAuthCookie(user.email, false);
                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.email,
                    DateTime.UtcNow,
                    DateTime.UtcNow.Add(FormsAuthentication.Timeout),
                    false,
                    string.Empty,
                    FormsAuthentication.FormsCookiePath);

                var encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection
                };
                this.HttpContext.Response.Cookies.Add(cookie);

                Session["UserID"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserFirstName"] = user.firstname;
                Session["UserLastName"] = user.lastname;
                Session["RoleID"] = user.roleID;
                Session["FullName"] = user.firstname + " " + user.lastname;

                // 🔥 PRIORITY: returnUrl (for booking flow)
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // fallback
                if (user.roleID == 1)
                    return RedirectToAction("AdminDashboard", "Main");
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
                return RedirectToAction("AdminDashboard", "Main");
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
        public async Task<ActionResult> GoogleSignIn(GoogleSignInRequest request)
        {
            try
            {
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

                // 🔒 Validate email first
                if (string.IsNullOrWhiteSpace(payload.Email))
                {
                    return Json(new { success = false, message = "Google account has no email." });
                }

                var email = payload.Email.Trim().ToLower();

                // 🔍 Find user
                var user = db.tbl_users.FirstOrDefault(u => u.googleSub == payload.Subject);

                if (user == null)
                {
                    user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == email);
                }

                // 🆕 CREATE USER (FIRST TIME GOOGLE LOGIN)
                if (user == null)
                {
                    user = new tbl_users
                    {
                        roleID = 2,
                        firstname = payload.GivenName ?? "Google",
                        lastname = payload.FamilyName ?? "User",
                        email = email,
                        password = "GOOGLE_LOGIN_ONLY",
                        coNum = "",
                        address = "",
                        googleSub = payload.Subject,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    };

                    db.tbl_users.Add(user);
                    db.SaveChanges(); // 🔥 IMPORTANT FIX
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user.googleSub))
                        user.googleSub = payload.Subject;

                    user.updatedAt = DateTime.Now;
                    db.SaveChanges();
                }

                // 🛡️ FINAL SAFETY CHECK
                if (user == null || string.IsNullOrWhiteSpace(user.email))
                {
                    return Json(new { success = false, message = "User creation failed." });
                }

                // ✅ LOGIN
                // Create and add forms-authentication cookie explicitly instead of
                // using FormsAuthentication.SetAuthCookie which depends on
                // HttpContext.Current and may be null after awaits.
                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.email,
                    DateTime.UtcNow,
                    DateTime.UtcNow.Add(FormsAuthentication.Timeout),
                    false,
                    string.Empty,
                    FormsAuthentication.FormsCookiePath);

                var encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection
                };
                this.HttpContext.Response.Cookies.Add(cookie);

                System.Diagnostics.Debug.WriteLine("Google Email: " + payload.Email);

                Session["UserID"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserFirstName"] = user.firstname;
                Session["UserLastName"] = user.lastname;
                Session["RoleID"] = user.roleID;
                Session["FullName"] = user.firstname + " " + user.lastname;

                string redirectUrl = user.roleID == 1
                    ? Url.Action("AdminDashboard", "Main")
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
                Session["ForgotPasswordOTPExpiry"] = DateTime.Now.AddMinutes(5);

                SendOtpEmail(user.email, user.firstname, otp, "reset");

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
        public ActionResult SendSignupOtp(string FNAME, string LNAME, string EMAIL,
    string CONTACT, string ADDRESS, string PASSWORD, string CONFIRMPASSWORD)
        {
            try
            {
                // Basic validations
                if (string.IsNullOrWhiteSpace(FNAME) || string.IsNullOrWhiteSpace(LNAME) ||
                    string.IsNullOrWhiteSpace(EMAIL) || string.IsNullOrWhiteSpace(CONTACT) ||
                    string.IsNullOrWhiteSpace(ADDRESS) || string.IsNullOrWhiteSpace(PASSWORD))
                {
                    TempData["ErrorMessage"] = "All fields are required.";
                    return RedirectToAction("SignupPage", "Main");
                }

                if (PASSWORD != CONFIRMPASSWORD)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return RedirectToAction("SignupPage", "Main");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(CONTACT, @"^\d{11}$"))
                {
                    TempData["ErrorMessage"] = "Contact number must be exactly 11 digits.";
                    return RedirectToAction("SignupPage", "Main");
                }

                bool hasLower = PASSWORD.Any(char.IsLower);
                bool hasUpper = PASSWORD.Any(char.IsUpper);
                bool hasDigit = PASSWORD.Any(char.IsDigit);
                bool hasSpecial = PASSWORD.Any(c => "@$!%*?&#".Contains(c));
                bool isLongEnough = PASSWORD.Length >= 8;

                if (!hasLower || !hasUpper || !hasDigit || !hasSpecial || !isLongEnough)
                {
                    TempData["ErrorMessage"] = "Password requirements not met.";
                    return RedirectToAction("SignupPage", "Main");
                }

                var existingUser = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == EMAIL.ToLower().Trim());
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email already registered. Please use a different email or login.";
                    return RedirectToAction("SignupPage", "Main");
                }

                // Generate and store OTP + form data in session
                var otp = new Random().Next(100000, 999999).ToString();

                Session["SignupOTP"] = otp;
                Session["SignupOTPExpiry"] = DateTime.Now.AddMinutes(5);
                Session["SignupFNAME"] = FNAME.Trim();
                Session["SignupLNAME"] = LNAME.Trim();
                Session["SignupEMAIL"] = EMAIL.Trim().ToLower();
                Session["SignupCONTACT"] = CONTACT.Trim();
                Session["SignupADDRESS"] = ADDRESS.Trim();
                Session["SignupPASSWORD"] = PASSWORD;

                SendOtpEmail(EMAIL.Trim(), FNAME.Trim(), otp, "signup");

                TempData["SignupShowOtpStep"] = true;
                TempData["SignupEmail"] = EMAIL.Trim();
                TempData["SuccessMessage"] = "A 6-digit OTP has been sent to your email.";

                return RedirectToAction("SignupPage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendSignupOtp Error: " + ex.ToString());
                TempData["ErrorMessage"] = "Failed to send OTP. Please try again.";
                return RedirectToAction("SignupPage", "Main");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifySignupOtp(string otp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(otp))
                {
                    TempData["ErrorMessage"] = "Please enter the OTP.";
                    TempData["SignupShowOtpStep"] = true;
                    TempData["SignupEmail"] = Session["SignupEMAIL"]?.ToString();
                    return RedirectToAction("SignupPage", "Main");
                }

                if (Session["SignupOTP"] == null || Session["SignupOTPExpiry"] == null)
                {
                    TempData["ErrorMessage"] = "OTP session expired. Please sign up again.";
                    return RedirectToAction("SignupPage", "Main");
                }

                var savedOtp = Session["SignupOTP"].ToString();
                var expiry = (DateTime)Session["SignupOTPExpiry"];

                if (DateTime.Now > expiry)
                {
                    Session.Remove("SignupOTP");
                    TempData["ErrorMessage"] = "OTP expired. Please sign up again.";
                    return RedirectToAction("SignupPage", "Main");
                }

                if (savedOtp != otp.Trim())
                {
                    TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
                    TempData["SignupShowOtpStep"] = true;
                    TempData["SignupEmail"] = Session["SignupEMAIL"]?.ToString();
                    return RedirectToAction("SignupPage", "Main");
                }

                // OTP valid — create the account
                string hashedPassword = HashPassword(Session["SignupPASSWORD"].ToString());

                var newUser = new tbl_users
                {
                    roleID = 2,
                    firstname = Session["SignupFNAME"].ToString(),
                    lastname = Session["SignupLNAME"].ToString(),
                    email = Session["SignupEMAIL"].ToString(),
                    password = hashedPassword,
                    coNum = Session["SignupCONTACT"].ToString(),
                    address = Session["SignupADDRESS"].ToString(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };

                db.tbl_users.Add(newUser);
                db.SaveChanges();

                // Clear signup session data
                Session.Remove("SignupOTP");
                Session.Remove("SignupOTPExpiry");
                Session.Remove("SignupFNAME");
                Session.Remove("SignupLNAME");
                Session.Remove("SignupEMAIL");
                Session.Remove("SignupCONTACT");
                Session.Remove("SignupADDRESS");
                Session.Remove("SignupPASSWORD");

                // Auto-login: set forms auth cookie
                var ticket = new FormsAuthenticationTicket(
                    1,
                    newUser.email,
                    DateTime.UtcNow,
                    DateTime.UtcNow.Add(FormsAuthentication.Timeout),
                    false,
                    string.Empty,
                    FormsAuthentication.FormsCookiePath);

                var encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection
                };
                Response.Cookies.Add(cookie);

                // Set session
                Session["UserID"] = newUser.userID;
                Session["UserEmail"] = newUser.email;
                Session["UserFirstName"] = newUser.firstname;
                Session["UserLastName"] = newUser.lastname;
                Session["RoleID"] = newUser.roleID;
                Session["FullName"] = newUser.firstname + " " + newUser.lastname;

                TempData["SuccessMessage"] = "Welcome to D'est Lounge! Your account has been created.";
                return RedirectToAction("Homepage", "Main");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VerifySignupOtp Error: " + ex.ToString());
                TempData["ErrorMessage"] = "A technical error occurred. Please try again.";
                TempData["SignupShowOtpStep"] = true;
                TempData["SignupEmail"] = Session["SignupEMAIL"]?.ToString();
                return RedirectToAction("SignupPage", "Main");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyForgotPasswordOtp(string email, string otp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                {
                    return Json(new { success = false, message = "Email and OTP are required." });
                }

                if (Session["ForgotPasswordOTP"] == null ||
                    Session["ForgotPasswordEmail"] == null ||
                    Session["ForgotPasswordOTPExpiry"] == null)
                {
                    return Json(new { success = false, message = "OTP session expired. Please request a new OTP." });
                }

                var savedOtp = Session["ForgotPasswordOTP"].ToString();
                var savedEmail = Session["ForgotPasswordEmail"].ToString();
                var expiry = (DateTime)Session["ForgotPasswordOTPExpiry"];

                if (DateTime.Now > expiry)
                {
                    Session.Remove("ForgotPasswordOTP");
                    Session.Remove("ForgotPasswordEmail");
                    Session.Remove("ForgotPasswordOTPExpiry");

                    return Json(new { success = false, message = "OTP expired. Please request a new one." });
                }

                if (savedEmail.ToLower().Trim() != email.ToLower().Trim() || savedOtp != otp.Trim())
                {
                    return Json(new { success = false, message = "Invalid OTP." });
                }

                return Json(new { success = true, message = "OTP verified successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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

        private void SendOtpEmail(string toEmail, string firstName, string otp, string type)
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
            var fromName = ConfigurationManager.AppSettings["SmtpFromName"];

            string subject = "";
            string body = "";

            if (type == "signup")
            {
                subject = "D'est Lounge Signup OTP";

                body = $@"Hello {firstName},

Your OTP for account registration is: {otp}

This OTP is valid for 5 minutes.

If you did not request this, please ignore this email.

- D'est Lounge";
            }
            else if (type == "reset")
            {
                subject = "D'est Lounge Password Reset OTP";

                body = $@"Hello {firstName},

Your OTP for password reset is: {otp}

This OTP is valid for 5 minutes.

If you did not request this, please ignore this email.

- D'est Lounge";
            }

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