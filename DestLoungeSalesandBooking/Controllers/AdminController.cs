using DestLoungeSalesandBooking.Filters;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    [SessionCheck(RequireAdmin = true)]
    public class AdminController : Controller
    {
        [HttpPost]
        public ActionResult SavePayment(string gcash, string bank)
        {
            try
            {
                gcash = (gcash ?? "").Trim();
                bank = (bank ?? "").Trim();

                if (string.IsNullOrWhiteSpace(gcash))
                {
                    return Json(new { success = false, message = "GCash number is required." });
                }

                if (!Regex.IsMatch(gcash, @"^09\d{9}$"))
                {
                    return Json(new { success = false, message = "GCash number must be 11 digits and start with 09." });
                }

                string filePath = Server.MapPath("~/App_Data/payment.txt");
                System.IO.File.WriteAllText(filePath, $"GCash:{gcash}\nBank:{bank}");

                return Json(new { success = true, message = "Payment settings saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetPaymentInfo()
        {
            try
            {
                string filePath = Server.MapPath("~/App_Data/payment.txt");

                string gcash = "";
                string bank = "";

                if (System.IO.File.Exists(filePath))
                {
                    var lines = System.IO.File.ReadAllLines(filePath);

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("GCash:", StringComparison.OrdinalIgnoreCase))
                        {
                            gcash = line.Substring("GCash:".Length).Trim();
                        }
                        else if (line.StartsWith("Bank:", StringComparison.OrdinalIgnoreCase))
                        {
                            bank = line.Substring("Bank:".Length).Trim();
                        }
                    }
                }

                return Json(new
                {
                    gcash = gcash,
                    bank = bank
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    gcash = "",
                    bank = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}