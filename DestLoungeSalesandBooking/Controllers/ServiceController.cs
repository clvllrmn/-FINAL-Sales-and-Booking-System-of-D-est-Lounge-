using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class ServiceController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET /Service/GetAllServices
        [HttpGet]
        public JsonResult GetAllServices()
        {
            try
            {
                var dbServices = db.tbl_services
                    .Where(s => s.is_active == 1)
                    .OrderBy(s => s.service_id)
                    .ToList();

                var services = dbServices.Select(s => new {
                    serviceId = s.service_id,
                    name = s.name,
                    description = s.description,
                    price = s.price,
                    category = s.category,
                    isActive = s.is_active,
                    image = FindServiceImageUrl(s.service_id)
                }).ToList();

                return Json(new { success = true, data = services },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateService(string name, string description,
                                decimal price, string category,
                                HttpPostedFileBase imageFile,
                                bool confirmDuplicate = false)  // ✅ add this param
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "Name is required." });

                // ✅ Duplicate check
                if (!confirmDuplicate)
                {
                    bool duplicateExists = db.tbl_services.Any(s =>
                        s.name.ToLower() == name.ToLower().Trim() && s.is_active == 1);

                    if (duplicateExists)
                        return Json(new
                        {
                            success = false,
                            isDuplicate = true,
                            message = "A service named \"" + name.Trim() + "\" already exists. Are you sure you want to add it anyway?"
                        });
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                        return Json(new { success = false, message = "Only image files are allowed." });

                    if (imageFile.ContentLength > 5 * 1024 * 1024)
                        return Json(new { success = false, message = "Image must be 5MB or less." });
                }

                var service = new tbl_services
                {
                    name = name.Trim(),
                    description = (description ?? "").Trim(),
                    price = price,
                    category = (category ?? "manicure").Trim(),
                    is_active = 1
                };

                db.tbl_services.Add(service);
                db.SaveChanges();

                if (imageFile != null && imageFile.ContentLength > 0)
                    SaveServiceImage(imageFile, service.service_id);

                return Json(new { success = true, message = "Service created successfully.", serviceId = service.service_id });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.InnerException?.Message
                            ?? ex.InnerException?.Message
                            ?? ex.Message;
                return Json(new { success = false, message = innerMsg });
            }
        }

        // POST /Service/UpdateService/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateService(int id, string name, string description,
                                        decimal price, string category,
                                        HttpPostedFileBase imageFile)
        {
            try
            {
                var service = db.tbl_services.Find(id);
                if (service == null)
                    return Json(new { success = false, message = "Service not found." });

                // ✅ Validate image BEFORE saving
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                        return Json(new { success = false, message = "Only image files (JPG, PNG, GIF, WEBP) are allowed." });

                    if (imageFile.ContentLength > 5 * 1024 * 1024)
                        return Json(new { success = false, message = "Image must be 5MB or less." });
                }

                service.name = name.Trim();
                service.description = (description ?? "").Trim();
                service.price = price;
                service.category = (category ?? "manicure").Trim();
                db.SaveChanges();

                if (imageFile != null && imageFile.ContentLength > 0)
                    SaveServiceImage(imageFile, service.service_id);

                return Json(new { success = true, message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Service/DeleteService/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteService(int id)
        {
            try
            {
                var service = db.tbl_services.Find(id);
                if (service == null)
                    return Json(new { success = false, message = "Service not found." });

                service.is_active = 0;
                db.SaveChanges();

                return Json(new { success = true, message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Service/RestoreService/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestoreService(int id)
        {
            try
            {
                var service = db.tbl_services.Find(id);
                if (service == null)
                    return Json(new { success = false, message = "Service not found." });

                service.is_active = 1;
                db.SaveChanges();

                return Json(new { success = true, message = "Service restored." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Service/GetDeletedServices
        [HttpGet]
        public JsonResult GetDeletedServices()
        {
            try
            {
                var services = db.tbl_services
                    .Where(s => s.is_active == 0)
                    .Select(s => new {
                        serviceId = s.service_id,
                        name = s.name,
                        description = s.description,
                        price = s.price,
                        category = s.category
                    })
                    .ToList();

                return Json(new { success = true, data = services },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // ---- helpers ----
        private void SaveServiceImage(HttpPostedFileBase file, int serviceId)
        {
            var folder = Server.MapPath("~/Content/ServiceImages/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var old in Directory.GetFiles(folder, serviceId + ".*"))
                System.IO.File.Delete(old);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fullPath = Path.Combine(folder, serviceId + ext);
            file.SaveAs(fullPath);
        }

        private string FindServiceImageUrl(int serviceId)
        {
            var folder = Server.MapPath("~/Content/ServiceImages/");
            if (!Directory.Exists(folder))
                return "/Content/Pictures/service-placeholder.jpg";

            var matches = Directory.GetFiles(folder, serviceId + ".*");
            if (matches.Length == 0)
                return "/Content/Pictures/service-placeholder.jpg";

            var fileName = Path.GetFileName(matches[0]);
            var timestamp = new FileInfo(matches[0]).LastWriteTime.Ticks;
            return "/Content/ServiceImages/" + fileName + "?t=" + timestamp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}