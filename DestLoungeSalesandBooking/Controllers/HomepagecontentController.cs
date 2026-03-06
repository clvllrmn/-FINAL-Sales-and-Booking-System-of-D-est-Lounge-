using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class HomePageContentController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();


        [HttpGet]
        public JsonResult TestConnection()
        {
            try
            {
                var test = db.tbl_homepage_content.Count();
                return Json(new { success = true, message = "Connection OK", count = test }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: Get all homepage content
        [HttpGet]
        public JsonResult GetAllContent()
        {
            try
            {
                var content = db.tbl_homepage_content
                    .Where(c => c.isActive)
                    .Select(c => new
                    {
                        c.contentID,
                        c.contentType,
                        c.contentValue,
                        c.updatedAt
                    })
                    .ToList();

                return Json(new { success = true, data = content }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllContent Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Update homepage content
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateContent(string contentType, string contentValue)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(contentValue))
                {
                    return Json(new { success = false, message = "ContentType and ContentValue are required" });
                }

                // Find the content by type
                var content = db.tbl_homepage_content
                    .FirstOrDefault(c => c.contentType == contentType.Trim() && c.isActive);

                if (content == null)
                {
                    // If not found, create a new one
                    var newContent = new tbl_homepage_content
                    {
                        contentType = contentType.Trim(),
                        contentValue = contentValue.Trim(),
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now,
                        isActive = true
                    };
                    db.tbl_homepage_content.Add(newContent);
                }
                else
                {
                    // Update existing
                    content.contentValue = contentValue.Trim();
                    content.updatedAt = DateTime.Now;
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateContent Error: " + ex.Message);
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}