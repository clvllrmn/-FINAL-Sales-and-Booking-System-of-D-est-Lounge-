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

        private static readonly string[] AllowedMime =
            { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxBytes = 5 * 1024 * 1024;

        // ── GET active photos ────────────────────────────────────────────
        [HttpGet]
        public JsonResult GetGalleryPhotos()
        {
            var photos = _db.tbl_gallery
                .Where(g => g.isActive && g.archivedAt == null)
                .OrderByDescending(g => g.createdAt)
                .Select(g => new {
                    g.galleryId,
                    g.caption,
                    g.description,
                    g.imageUrl,
                    g.createdAt
                }).ToList();
            return Json(new { success = true, data = photos }, JsonRequestBehavior.AllowGet);
        }

        // ── GET archived photos ──────────────────────────────────────────
        [HttpGet]
        public JsonResult GetArchivedPhotos()
        {
            var photos = _db.tbl_gallery
                .Where(g => g.archivedAt != null)
                .OrderByDescending(g => g.archivedAt)
                .Select(g => new {
                    g.galleryId,
                    g.caption,
                    g.description,
                    g.imageUrl,
                    g.archivedAt
                }).ToList();
            return Json(new { success = true, data = photos }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { success = false, message = "Only JPG, PNG, WEBP, or GIF allowed." });
                if (imageFile.ContentLength > MaxBytes)
                    return Json(new { success = false, message = "File exceeds 5 MB limit." });

                var url = SaveImageFile(imageFile);

                var record = new tbl_gallery
                {
                    caption = caption.Trim(),
                    description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    imageUrl = url,
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
                    galleryId = record.galleryId,
                    imageUrl = record.imageUrl,
                    caption = record.caption
                });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST update caption/description + optional new image ─────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCaption(int galleryId, string caption, string description,
                                        HttpPostedFileBase newImageFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caption))
                    return Json(new { success = false, message = "Caption cannot be empty." });

                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null || photo.archivedAt != null)
                    return Json(new { success = false, message = "Photo not found." });

                photo.caption = caption.Trim();
                photo.description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                photo.updatedAt = DateTime.Now;

                // Replace image if a new file was provided
                if (newImageFile != null && newImageFile.ContentLength > 0)
                {
                    if (!AllowedMime.Contains(newImageFile.ContentType.ToLower()))
                        return Json(new { success = false, message = "Only JPG, PNG, WEBP, or GIF allowed." });
                    if (newImageFile.ContentLength > MaxBytes)
                        return Json(new { success = false, message = "File exceeds 5 MB limit." });

                    // Delete old file from disk
                    DeleteImageFile(photo.imageUrl);

                    photo.imageUrl = SaveImageFile(newImageFile);
                    photo.fileName = newImageFile.FileName;
                    photo.fileSizeBytes = newImageFile.ContentLength;
                }

                _db.SaveChanges();
                return Json(new { success = true, imageUrl = photo.imageUrl });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST soft-archive ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeletePhoto(int galleryId)
        {
            try
            {
                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null) return Json(new { success = false, message = "Photo not found." });
                photo.isActive = false;
                photo.archivedAt = DateTime.Now;
                photo.updatedAt = DateTime.Now;
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST restore from archive ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestorePhoto(int galleryId)
        {
            try
            {
                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null) return Json(new { success = false, message = "Photo not found." });
                photo.isActive = true;
                photo.archivedAt = null;
                photo.updatedAt = DateTime.Now;
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST permanent delete ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PermanentDeletePhoto(int galleryId)
        {
            try
            {
                var photo = _db.tbl_gallery.Find(galleryId);
                if (photo == null) return Json(new { success = false, message = "Photo not found." });
                DeleteImageFile(photo.imageUrl);
                _db.tbl_gallery.Remove(photo);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private string SaveImageFile(HttpPostedFileBase file)
        {
            var folder = Server.MapPath("~/Content/Gallery/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var ext = Path.GetExtension(file.FileName);
            var name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, name));
            return "/Content/Gallery/" + name;
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            try
            {
                var physical = Server.MapPath("~" + imageUrl);
                if (System.IO.File.Exists(physical))
                    System.IO.File.Delete(physical);
            }
            catch { /* non-fatal */ }
        }
    }
}