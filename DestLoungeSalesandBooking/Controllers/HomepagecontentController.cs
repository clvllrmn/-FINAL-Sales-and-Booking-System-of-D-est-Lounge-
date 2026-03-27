using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.IO;
using System.Linq;
using System.Web;
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

        // POST: Update homepage text content
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateContent(string contentType, string contentValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(contentValue))
                    return Json(new { success = false, message = "ContentType and ContentValue are required" });

                var content = db.tbl_homepage_content
                    .FirstOrDefault(c => c.contentType == contentType.Trim() && c.isActive);

                if (content == null)
                {
                    db.tbl_homepage_content.Add(new tbl_homepage_content
                    {
                        contentType = contentType.Trim(),
                        contentValue = contentValue.Trim(),
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now,
                        isActive = true
                    });
                }
                else
                {
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

        // POST: Upload a polaroid image (slot 1, 2, or 3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdatePolaroidImage(int slot, HttpPostedFileBase imageFile)
        {
            try
            {
                if (slot < 1 || slot > 3)
                    return Json(new { success = false, message = "Invalid slot. Must be 1, 2, or 3." });

                if (imageFile == null || imageFile.ContentLength == 0)
                    return Json(new { success = false, message = "No image provided." });

                // 2MB limit
                if (imageFile.ContentLength > 2 * 1024 * 1024)
                    return Json(new { success = false, message = "Image must be 2MB or less." });

                // Save file to ~/Content/Pictures/polaroid_{slot}.{ext}
                var folder = Server.MapPath("~/Content/Pictures/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Delete old polaroid file for this slot regardless of extension
                foreach (var old in Directory.GetFiles(folder, "polaroid_" + slot + ".*"))
                    System.IO.File.Delete(old);

                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                var fileName = "polaroid_" + slot + ext;
                imageFile.SaveAs(Path.Combine(folder, fileName));

                var imagePath = "/Content/Pictures/" + fileName + "?t=" + DateTime.Now.Ticks;

                // Update the DB record
                var contentType = "polaroid_" + slot;
                var content = db.tbl_homepage_content
                    .FirstOrDefault(c => c.contentType == contentType && c.isActive);

                if (content == null)
                {
                    db.tbl_homepage_content.Add(new tbl_homepage_content
                    {
                        contentType = contentType,
                        contentValue = "/Content/Pictures/" + fileName,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now,
                        isActive = true
                    });
                }
                else
                {
                    content.contentValue = "/Content/Pictures/" + fileName;
                    content.updatedAt = DateTime.Now;
                }

                db.SaveChanges();

                return Json(new { success = true, imageUrl = imagePath });
            }
            catch (Exception ex)
            {
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