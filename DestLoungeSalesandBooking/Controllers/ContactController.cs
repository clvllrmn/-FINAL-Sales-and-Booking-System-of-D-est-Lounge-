using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class ContactController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET: /Contact/GetAllContact
        [HttpGet]
        public JsonResult GetAllContact()
        {
            try
            {
                var contacts = db.tbl_contact
                    .Where(c => c.isActive)
                    .OrderBy(c => c.createdAt)
                    .Select(c => new
                    {
                        contactID = c.contactID,
                        infoType = c.infoType,
                        label = c.label,
                        value = c.value,
                        icon = c.icon,
                        createdAt = c.createdAt,
                        updatedAt = c.updatedAt
                    })
                    .ToList();

                return Json(new { success = true, data = contacts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Contact/GetContactById/1
        [HttpGet]
        public JsonResult GetContactById(int id)
        {
            try
            {
                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id && c.isActive);

                if (contact == null)
                    return Json(new { success = false, message = "Contact not found" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        contactID = contact.contactID,
                        infoType = contact.infoType,
                        label = contact.label,
                        value = contact.value,
                        icon = contact.icon,
                        createdAt = contact.createdAt,
                        updatedAt = contact.updatedAt
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetContactById Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Contact/CreateContact
        [HttpPost]
        public JsonResult CreateContact(string infoType, string label, string value, string icon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(infoType) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                    return Json(new { success = false, message = "InfoType, Label, and Value are required" });

                var existingContact = db.tbl_contact
                    .FirstOrDefault(c => c.infoType.ToLower() == infoType.ToLower().Trim() && c.isActive);

                if (existingContact != null)
                    return Json(new { success = false, message = "This contact info type already exists" });

                var newContact = new tbl_contact
                {
                    infoType = infoType.Trim(),
                    label = label.Trim(),
                    value = value.Trim(),
                    icon = icon ?? "",
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    isActive = true
                };

                db.tbl_contact.Add(newContact);
                db.SaveChanges();

                return Json(new { success = true, message = "Contact info created successfully", contactID = newContact.contactID });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/UpdateContact/1
        [HttpPost]
        public JsonResult UpdateContact(int id, string infoType, string label, string value, string icon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(infoType) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                    return Json(new { success = false, message = "InfoType, Label, and Value are required" });

                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id);

                if (contact == null)
                    return Json(new { success = false, message = "Contact info not found" });

                var duplicateInfoType = db.tbl_contact
                    .FirstOrDefault(c => c.infoType.ToLower() == infoType.ToLower().Trim() && c.contactID != id && c.isActive);

                if (duplicateInfoType != null)
                    return Json(new { success = false, message = "This contact info type already exists" });

                contact.infoType = infoType.Trim();
                contact.label = label.Trim();
                contact.value = value.Trim();
                contact.icon = icon ?? "";
                contact.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "Contact info updated successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/DeleteContact/1
        [HttpPost]
        public JsonResult DeleteContact(int id)
        {
            try
            {
                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id);

                if (contact == null)
                    return Json(new { success = false, message = "Contact info not found" });

                contact.isActive = false;
                contact.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "Contact info deleted successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}