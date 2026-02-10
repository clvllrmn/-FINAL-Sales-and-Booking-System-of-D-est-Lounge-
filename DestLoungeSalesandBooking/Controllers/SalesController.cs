using System;
using System.Linq;
using System.Web.Mvc;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System.Data.Entity;
using MySql.Data.MySqlClient;
using System.Configuration;



namespace DestLoungeSalesandBooking.Controllers
{
    public class SalesController : Controller
    {
        private readonly DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET: /Sales/List
        public ActionResult List()
        {
            var items = db.tbl_sales
                .OrderByDescending(s => s.CreatedAt)
                .Take(50)
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        // POST: /Sales/Create
        // Creates empty sale header. Items are added via AddItem.
        [HttpPost]
        public ActionResult Create(int? bookingId, int? customerId, string paymentMethod = "Cash", decimal discount = 0)
        {
            var sale = new tbl_sales
            {
                BookingId = bookingId,
                CustomerId = customerId,
                PaymentMethod = paymentMethod,
                Discount = discount,
                Status = "Paid",
                Subtotal = 0,
                Total = 0,
                CreatedAt = DateTime.Now
            };

            db.tbl_sales.Add(sale);
            db.SaveChanges();

            return Json(new { success = true, saleId = sale.SaleId });
        }

        // POST: /Sales/AddItem
        // Adds item and recomputes totals.
        [HttpPost]
        public ActionResult AddItem(int saleId, string itemType, int itemId, string itemName, int qty, decimal unitPrice)
        {
            if (qty <= 0) return Json(new { success = false, message = "Qty must be >= 1" });
            if (unitPrice < 0) return Json(new { success = false, message = "UnitPrice cannot be negative" });

            itemType = (itemType ?? "").Trim();

            if (itemType != "Service" && itemType != "Product")
                return Json(new { success = false, message = "ItemType must be 'Service' or 'Product'." });

            var sale = db.tbl_sales.FirstOrDefault(s => s.SaleId == saleId);
            if (sale == null) return Json(new { success = false, message = "Sale not found." });

            var lineTotal = qty * unitPrice;

            var item = new tbl_sale_items
            {
                SaleId = saleId,
                ItemType = itemType,
                ItemId = itemId,
                ItemName = itemName ?? "",
                Qty = qty,
                UnitPrice = unitPrice,
                LineTotal = lineTotal
            };

            db.tbl_sale_items.Add(item);
            db.SaveChanges();

            // Recompute totals
            var subtotal = db.tbl_sale_items
                .Where(i => i.SaleId == saleId)
                .Select(i => (decimal?)i.LineTotal)
                .Sum() ?? 0;

            sale.Subtotal = subtotal;
            sale.Total = Math.Max(0, sale.Subtotal - sale.Discount);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Item added.",
                subtotal = sale.Subtotal,
                discount = sale.Discount,
                total = sale.Total
            });
        }

        // GET: /Sales/Details?saleId=1
        public ActionResult Details(int saleId)
        {
            var sale = db.tbl_sales.FirstOrDefault(s => s.SaleId == saleId);
            if (sale == null) return Content("Sale not found.");

            var items = db.tbl_sale_items.Where(i => i.SaleId == saleId).ToList();

            return Json(new { sale, items }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Sales/TestCreate
        public ActionResult TestCreate()
        {
            var sale = new tbl_sales
            {
                BookingId = 1,
                CustomerId = 1,
                PaymentMethod = "Cash",
                Discount = 0,
                Status = "Paid",
                Subtotal = 0,
                Total = 0,
                CreatedAt = DateTime.Now
            };

            db.tbl_sales.Add(sale);
            db.SaveChanges();

            return Content("Created sale. SaleId = " + sale.SaleId);
        }

        // GET: /Sales/TestAddItem?saleId=1
        public ActionResult TestAddItem(int saleId)
        {
            var item = new tbl_sale_items
            {
                SaleId = saleId,
                ItemType = "Service",
                ItemId = 1,
                ItemName = "Test Service",
                Qty = 1,
                UnitPrice = 250,
                LineTotal = 250
            };

            db.tbl_sale_items.Add(item);
            db.SaveChanges();

            var sale = db.tbl_sales.First(s => s.SaleId == saleId);
            var subtotal = db.tbl_sale_items.Where(i => i.SaleId == saleId).Sum(i => i.LineTotal);
            sale.Subtotal = subtotal;
            sale.Total = Math.Max(0, sale.Subtotal - sale.Discount);
            db.SaveChanges();

            return Content("Added item. Subtotal=" + sale.Subtotal + " Total=" + sale.Total);
        }


        // GET: /Sales/Daily?days=30
        public ActionResult Daily(int days = 30)
        {
            var from = DateTime.Today.AddDays(-days + 1);

            // Pull from DB first (MySQL provider limitations)
            var sales = db.tbl_sales
                .Where(s => s.CreatedAt >= from)
                .ToList();

            var data = sales
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    SalesCount = g.Count(),
                    TotalSales = g.Sum(x => x.Total),
                    TotalDiscount = g.Sum(x => x.Discount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        // GET: /Sales/Weekly?weeks=8
        public ActionResult Weekly(int weeks = 8)
        {
            var from = DateTime.Today.AddDays(-(weeks * 7) + 1);

            var sales = db.tbl_sales
                .Where(s => s.CreatedAt >= from)
                .ToList();

            var data = sales
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    TotalSales = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        // GET: /Sales/SeedSales
        public ActionResult SeedSales()
        {
            var cs = ConfigurationManager.ConnectionStrings["db_destloungesaleandbooking"].ConnectionString;

            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();

                // Insert 5 rows, different dates
                for (int i = 0; i < 5; i++)
                {
                    var created = DateTime.Today.AddDays(-i).AddHours(10);

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                    INSERT INTO tbl_sales
                    (BookingId, CustomerId, Subtotal, Discount, Total, PaymentMethod, Status, CreatedAt)
                    VALUES
                    (@BookingId, @CustomerId, @Subtotal, @Discount, @Total, @PaymentMethod, @Status, @CreatedAt); ";
                    cmd.Parameters.AddWithValue("@BookingId", 1);
                    cmd.Parameters.AddWithValue("@CustomerId", 1);
                    cmd.Parameters.AddWithValue("@Subtotal", 500 + (i * 50));
                    cmd.Parameters.AddWithValue("@Discount", 0);
                    cmd.Parameters.AddWithValue("@Total", 500 + (i * 50));
                    cmd.Parameters.AddWithValue("@PaymentMethod", "Cash");
                    cmd.Parameters.AddWithValue("@Status", "Paid");
                    cmd.Parameters.AddWithValue("@CreatedAt", created);

                    cmd.ExecuteNonQuery();
                }
            }

            return Content("Seeded 5 sales rows.");
        }


        // GET: /Sales/Count
        public ActionResult Count()
        {
            var count = db.tbl_sales.Count();
            return Content("tbl_sales rows = " + count);
        }



    }
}
