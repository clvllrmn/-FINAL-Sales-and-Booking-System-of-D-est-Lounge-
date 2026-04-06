using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    [SessionCheck(RequireAdmin = true)]
    public class AdminController : Controller
    {
        private readonly DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        [HttpPost]
        [SessionCheck(RequireAdmin = true)]
        public ActionResult SavePayment(string gcash, string bank, HttpPostedFileBase qr)
        {
            try
            {
                gcash = (gcash ?? "").Trim();
                bank = (bank ?? "").Trim();

                if (string.IsNullOrWhiteSpace(gcash))
                {
                    return Json(new { success = false, message = "GCash is required." });
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(gcash, @"^\d{11}$"))
                {
                    return Json(new { success = false, message = "GCash must be exactly 11 digits." });
                }

                string qrPath = null;

                if (qr != null && qr.ContentLength > 0)
                {
                    var ext = Path.GetExtension(qr.FileName).ToLower();

                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                    {
                        return Json(new { success = false, message = "Only PNG, JPG, and JPEG are allowed." });
                    }

                    var folder = Server.MapPath("~/Uploads/PaymentQR");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var fullPath = Path.Combine(folder, fileName);

                    qr.SaveAs(fullPath);

                    qrPath = "/Uploads/PaymentQR/" + fileName;
                }

                var payment = db.tbl_payment_settings.FirstOrDefault();

                if (payment == null)
                {
                    payment = new tbl_payment_settings
                    {
                        GCash = gcash,
                        Bank = bank,
                        QRCodePath = qrPath,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.tbl_payment_settings.Add(payment);
                }
                else
                {
                    payment.GCash = gcash;
                    payment.Bank = bank;

                    if (!string.IsNullOrWhiteSpace(qrPath))
                    {
                        payment.QRCodePath = qrPath;
                    }

                    payment.UpdatedAt = DateTime.Now;
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Payment settings saved successfully.",
                    qr = payment.QRCodePath
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
