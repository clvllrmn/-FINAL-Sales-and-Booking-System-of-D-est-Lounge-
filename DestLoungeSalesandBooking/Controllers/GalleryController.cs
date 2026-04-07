// Controllers/GalleryController.cs
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class GalleryController : Controller
    {
        private readonly DestLoungeSalesandBookingContext _db = new DestLoungeSalesandBookingContext();

        // ── Allowed image types ──────────────────────────────────────────
        private static readonly string[] AllowedMime =
            { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        // ── GET all active photos (admin + public) ───────────────────────
        [HttpGet]
        public JsonResult GetGalleryPhotos()
        {
            var photos = _db.tbl_gallery
                .Where(g => g.isActive)
                .OrderByDescending(g => g.createdAt)
                .Select(g => new
                {
                    g.galleryId,
                    g.caption,
                    g.description,
                    g.imageUrl,
                    g.createdAt
                })
                .ToList();

            return Json(new { success = true, data = photos },
                        JsonRequestBehavior.AllowGet);
        }

        // ── POST upload new photo ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadPhoto(string caption, string description, HttpPostedFileBase imageFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caption))
                    return Json(new { success = false, message = "Caption is required." });

                if (imageFile == null || imageFile.ContentLength == 0)
                    return Json(new { success = false, message = "No file uploaded." });

                if (!AllowedMime.Contains(imageFile.ContentType.ToLower()))
                    return Json(new { success = false, message = "Only JPG, PNG, WEBP, or GIF images are allowed." });

                if (imageFile.ContentLength > MaxBytes)
                    return Json(new { success = false, message = "File exceeds the 5 MB limit." });

                // Save to ~/Content/Gallery/
                var galleryFolder = Server.MapPath("~/Content/Gallery/");
                if (!Directory.Exists(galleryFolder))
                    Directory.CreateDirectory(galleryFolder);

                var ext = Path.GetExtension(imageFile.FileName);
                var safeName = Guid.NewGuid().ToString("N") + ext;
                var fullPath = Path.Combine(galleryFolder, safeName);
                imageFile.SaveAs(fullPath);

                var record = new tbl_gallery
                {
                    caption = caption.Trim(),
                    description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    imageUrl = "/Content/Gallery/" + safeName,
                    fileName = imageFile.FileName,
                    fileSizeBytes = imageFile.ContentLength,
                    isActive = true,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };

                _db.tbl_gallery.Add(record);
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Photo uploaded successfully.",
                    galleryId = record.galleryId,
                    imageUrl = record.imageUrl,
                    caption = record.caption
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Upload failed: " + ex.Message });
            }
        }

        // ── POST edit caption ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCaption(int galleryId, string caption, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caption))
                    return Json(new { success = false, message = "Caption cannot be empty." });

                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null || !photo.isActive)
                    return Json(new { success = false, message = "Photo not found." });

                photo.caption = caption.Trim();
                photo.description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                photo.updatedAt = DateTime.Now;
                _db.SaveChanges();

                return Json(new { success = true, message = "Caption updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST soft-delete (sets isActive = false) ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeletePhoto(int galleryId)
        {
            try
            {
                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null)
                    return Json(new { success = false, message = "Photo not found." });

                photo.isActive = false;
                photo.updatedAt = DateTime.Now;
                _db.SaveChanges();

                return Json(new { success = true, message = "Photo deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}