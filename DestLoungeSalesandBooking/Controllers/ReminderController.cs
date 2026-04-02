using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    [SessionCheck(RequireAdmin = true)]
    public class ReminderController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();
        private const string SecretKey = "dest123";

        public ActionResult SendOneDayReminders(string key)
        {
            if (key != SecretKey)
            {
                return Content("Unauthorized");
            }

            DateTime tomorrow = DateTime.Today.AddDays(1);

            var bookings = db.tbl_bookings
                .Where(b => b.BookingDate.Year == tomorrow.Year
                         && b.BookingDate.Month == tomorrow.Month
                         && b.BookingDate.Day == tomorrow.Day
                         && !b.ReminderSent
                         && b.Status == "Approved")
                .ToList();

            int sentCount = 0;

            foreach (var booking in bookings)
            {
                var customer = db.tbl_users.FirstOrDefault(u => u.userID == booking.CustomerId);
                var service = db.tbl_services.FirstOrDefault(s => s.service_id == booking.ServiceId);

                if (customer == null) continue;
                if (string.IsNullOrWhiteSpace(customer.email)) continue;

                string fullName = (customer.firstname + " " + customer.lastname).Trim();
                string serviceName = service != null ? service.name : "your booked service";
                string timeText = DateTime.Today.Add(booking.StartTime).ToString("hh:mm tt");

                string subject = "Appointment Reminder - D'est Lounge";
                string body =
                    "Hello " + fullName + ",\n\n" +
                    "This is a friendly reminder that you have an appointment tomorrow.\n\n" +
                    "Service: " + serviceName + "\n" +
                    "Date: " + booking.BookingDate.ToString("MMMM dd, yyyy") + "\n" +
                    "Time: " + timeText + "\n\n" +
                    "Please arrive on time.\n\n" +
                    "Thank you,\n" +
                    "D'est Lounge";

                try
                {
                    SendEmail(customer.email, subject, body);
                    booking.ReminderSent = true;
                    sentCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("1-day reminder error: " + ex.Message);
                }
            }

            db.SaveChanges();

            return Content("1-day reminder process completed. Emails sent: " + sentCount);
        }

        public ActionResult SendThreeHourReminders(string key)
        {
            if (key != SecretKey)
            {
                return Content("Unauthorized");
            }

            DateTime now = DateTime.Now;

            var bookings = db.tbl_bookings
                .Where(b => !b.Reminder3HoursSent && b.Status == "Approved")
                .ToList();

            int sentCount = 0;

            foreach (var booking in bookings)
            {
                var customer = db.tbl_users.FirstOrDefault(u => u.userID == booking.CustomerId);
                var service = db.tbl_services.FirstOrDefault(s => s.service_id == booking.ServiceId);

                if (customer == null) continue;
                if (string.IsNullOrWhiteSpace(customer.email)) continue;

                DateTime appointmentDateTime = booking.BookingDate.Date.Add(booking.StartTime);
                TimeSpan remainingTime = appointmentDateTime - now;

                if (remainingTime.TotalMinutes > 0 && remainingTime.TotalHours <= 3)
                {
                    string fullName = (customer.firstname + " " + customer.lastname).Trim();
                    string serviceName = service != null ? service.name : "your booked service";

                    string subject = "Appointment in 3 Hours - D'est Lounge";
                    string body =
                        "Hello " + fullName + ",\n\n" +
                        "This is a reminder that your appointment is in less than 3 hours.\n\n" +
                        "Service: " + serviceName + "\n" +
                        "Date: " + booking.BookingDate.ToString("MMMM dd, yyyy") + "\n" +
                        "Time: " + appointmentDateTime.ToString("hh:mm tt") + "\n\n" +
                        "Please be ready and arrive on time.\n\n" +
                        "Thank you,\n" +
                        "D'est Lounge";

                    try
                    {
                        SendEmail(customer.email, subject, body);
                        booking.Reminder3HoursSent = true;
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("3-hour reminder error: " + ex.Message);
                    }
                }
            }

            db.SaveChanges();

            return Content("3-hour reminder process completed. Emails sent: " + sentCount);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
            string smtpFromName = ConfigurationManager.AppSettings["SmtpFromName"];

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(smtpEmail, smtpFromName);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpEmail, smtpPass);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}