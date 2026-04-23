    using DestLoungeSalesandBooking.Filters;
    using DestLoungeSalesandBooking.Models;
    using DestLoungeSalesandBooking.Models.Context;
    using System;
    using System.Collections.Generic;
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


        [HttpGet]
        public JsonResult GetSalesAnalytics(
        string range = "today",
        string from = null,
        string to = null)
                {
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Now;

            switch ((range ?? "today").ToLower())
            {
                case "month":
                case "thismonth":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;

                case "3months":
                    start = DateTime.Today.AddMonths(-3);
                    break;

                case "6months":
                    start = DateTime.Today.AddMonths(-6);
                    break;

                case "year":
                case "thisyear":
                    start = new DateTime(DateTime.Today.Year, 1, 1);
                    break;

                case "custom":

                    DateTime tempStart;
                    DateTime tempEnd;

                    if (
                        DateTime.TryParseExact(
                            from,
                            "dd/MM/yyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out tempStart
                        )
                        &&
                        DateTime.TryParseExact(
                            to,
                            "dd/MM/yyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out tempEnd
                        )
                    )
                    {
                        start = tempStart.Date;
                        end = tempEnd.Date.AddDays(1).AddSeconds(-1);
                    }

                    break;


                default:
                    start = DateTime.Today;
                    break;
            }

            var sales = db.tbl_sales
                .Where(x => x.CreatedAt >= start &&
                            x.CreatedAt <= end &&
                            x.Status == "Paid")
                .ToList();

            var saleIds = sales.Select(x => x.SaleId).ToList();

            var items = db.tbl_sale_items
                .Where(x => saleIds.Contains(x.SaleId))
                .ToList();

            decimal totalSales = sales.Sum(x => (decimal?)x.Total) ?? 0;
            int completedBookings = sales.Count();

            var topService = items
                .GroupBy(x => x.ItemName)
                .Select(g => new {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var serviceTable = items
                .GroupBy(x => x.ItemName)
                .Select(g => new
                {
                    Service = g.Key,
                    Bookings = g.Count(),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .ToList();

            var points = sales
                .GroupBy(x => x.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    label = g.Key.ToString("MMM dd"),
                    value = g.Sum(x => x.Total)
                })
                .ToList();

            return Json(new
            {
                totalRevenue = totalSales,
                completedBookings = completedBookings,
                topService = topService != null ? topService.Name : "-",
                services = serviceTable,
                points = points
            }, JsonRequestBehavior.AllowGet);
        }


    }
    
    }
