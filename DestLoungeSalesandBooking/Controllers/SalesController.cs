using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class SalesController : Controller
    {
        private readonly DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET: /Sales/Analytics?range=today|week|month
        [HttpGet]
        public ActionResult Analytics(string range = "week")
        {
            range = (range ?? "week").Trim().ToLowerInvariant();

            DateTime today = DateTime.Today;
            DateTime start;
            DateTime endExclusive;

            if (range == "today")
            {
                start = today;
                endExclusive = today.AddDays(1);
            }
            else if (range == "month")
            {
                start = new DateTime(today.Year, today.Month, 1);
                endExclusive = start.AddMonths(1);
            }
            else // default: week
            {
                // Monday-based week
                int diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday);
                if (diff < 0) diff += 7;
                start = today.AddDays(-diff);
                endExclusive = start.AddDays(7);
                range = "week";
            }

            var completed = db.tbl_bookings
                .Where(b => b.Status == "Completed" && b.BookingDate >= start && b.BookingDate < endExclusive)
                .ToList();

            // group by date
            var byDay = completed
                .GroupBy(b => b.BookingDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => ExtractDownpayment(x.Notes))
                })
                .ToList();

            // build chart points
            var points = new List<object>();

            if (range == "today")
            {
                // single point
                decimal rev = byDay.Sum(x => x.Revenue);
                points.Add(new { label = today.ToString("ddd", CultureInfo.InvariantCulture), value = rev });
            }
            else if (range == "week")
            {
                // always 7 days Mon-Sun (even if 0)
                for (int i = 0; i < 7; i++)
                {
                    var d = start.AddDays(i).Date;
                    var found = byDay.FirstOrDefault(x => x.Date == d);
                    points.Add(new
                    {
                        label = d.ToString("ddd", CultureInfo.InvariantCulture),
                        value = (found != null ? found.Revenue : 0m)
                    });
                }
            }
            else // month
            {
                int days = DateTime.DaysInMonth(start.Year, start.Month);
                for (int day = 1; day <= days; day++)
                {
                    var d = new DateTime(start.Year, start.Month, day);
                    var found = byDay.FirstOrDefault(x => x.Date == d);
                    points.Add(new
                    {
                        label = day.ToString(),
                        value = (found != null ? found.Revenue : 0m)
                    });
                }
            }

            decimal totalRevenue = byDay.Sum(x => x.Revenue);
            int totalBookings = completed.Count;

            return Json(new
            {
                range,
                start = start.ToString("yyyy-MM-dd"),
                endExclusive = endExclusive.ToString("yyyy-MM-dd"),
                totalRevenue,
                totalBookings,
                points
            }, JsonRequestBehavior.AllowGet);
        }

        // Notes example contains: "Downpayment: 479"
        private decimal ExtractDownpayment(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return 0m;

            var match = Regex.Match(notes, @"Downpayment:\s*([0-9]+(\.[0-9]+)?)", RegexOptions.IgnoreCase);
            if (!match.Success) return 0m;

            if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                return v;

            return 0m;
        }
    }
}