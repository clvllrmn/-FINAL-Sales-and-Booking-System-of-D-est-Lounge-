using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace DestLoungeSalesandBooking.Controllers
{
    public class AccountController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

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

                var user = db.tbl_users.FirstOrDefault(u => u.email.ToLower() == email.ToLower().Trim());

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return RedirectToAction("LoginPage", "Main");
                }

                string hashedPassword = HashPassword(password);

                if (user.password != hashedPassword)
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return RedirectToAction("LoginPage", "Main");
                }

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
            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToAction("LoginPage", "Main");
        }

        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return RedirectToAction("ForgotPasswordPage", "Main");
        }

        // POST: Account/ResetPassword (For regular users)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(currentPassword) ||
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
                    TempData["ErrorMessage"] = "Invalid email or current password.";
                    return RedirectToAction("ForgotPasswordPage", "Main");
                }

                string hashedCurrentPassword = HashPassword(currentPassword);
                if (user.password != hashedCurrentPassword)
                {
                    TempData["ErrorMessage"] = "Invalid email or current password.";
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

        // GET: Account/ChangePassword (Admin only)
        public ActionResult ChangePassword()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("LoginPage", "Main");
            }
            return RedirectToAction("AdminChangePasswordPage", "Main");
        }

        // POST: Account/ChangePassword (Admin only)
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
    }
}