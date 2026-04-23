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
                case "today":
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;

                case "month":
                case "thismonth":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    end = start.AddMonths(1).AddSeconds(-1);
                    break;

                case "3months":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);
                    end = DateTime.Now;
                    break;

                case "6months":
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
                    end = DateTime.Now;
                    break;

                case "year":
                case "thisyear":
                    start = new DateTime(DateTime.Today.Year, 1, 1);
                    end = new DateTime(DateTime.Today.Year, 12, 31, 23, 59, 59);
                    break;

                case "custom":

                    if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                    {
                        start = DateTime.Today;
                        end = DateTime.Now;
                        break;
                    }

                    DateTime parsedFrom;
                    DateTime parsedTo;

                    if (!DateTime.TryParse(from, out parsedFrom) ||
                        !DateTime.TryParse(to, out parsedTo))
                    {
                        start = DateTime.Today;
                        end = DateTime.Now;
                        break;
                    }

                    start = parsedFrom.Date;
                    end = parsedTo.Date.AddDays(1);

                    break;





                default:
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }


                var sales = (
                from s in db.tbl_sales
                join b in db.tbl_bookings
                on s.BookingId equals b.BookingId
                where b.BookingDate >= start
                && b.BookingDate < end
                && s.Status == "Paid"
                select new
                {
                    s.SaleId,
                    s.Total,
                    BookingDate = b.BookingDate
                }
            ).ToList();

            var saleIds = sales.Select(x => x.SaleId).ToList();

            var items = db.tbl_sale_items
                .Where(x => saleIds.Contains(x.SaleId))
                .ToList();

            decimal totalSales = sales.Sum(x => (decimal?)x.Total) ?? 0;
            int completedBookings = sales.Count();

            var topService = items
                .GroupBy(x => x.ItemName)
                .Select(g => new
                {
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
                .GroupBy(x => x.BookingDate.Date)
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
