using DestLoungeSalesandBooking.Filters;
using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    [SessionCheck(RequireAdmin = true)]
    public class ContactController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET: /Contact/GetAllContact - Returns all active contacts
        [HttpGet]
        public JsonResult GetAllContact()
        {
            try
            {
                var contacts = db.tbl_contact
                    .Where(c => c.isActive)
                    .OrderBy(c => c.contactID)
                    .Select(c => new
                    {
                        contactID = c.contactID,
                        infoType = c.infoType,
                        label = c.label,
                        value = c.value,
                        icon = c.icon
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

        // GET: /Contact/GetDeletedContacts - Returns all soft-deleted contacts
        [HttpGet]
        public JsonResult GetDeletedContacts()
        {
            try
            {
                var contacts = db.tbl_contact
                    .Where(c => !c.isActive)
                    .OrderByDescending(c => c.updatedAt)
                    .Select(c => new
                    {
                        contactID = c.contactID,
                        infoType = c.infoType,
                        label = c.label,
                        value = c.value,
                        icon = c.icon
                    })
                    .ToList();

                return Json(new { success = true, data = contacts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetDeletedContacts Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Contact/CreateContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateContact(string infoType, string label, string value, string icon)
        {
            try
            {
                // ── Only label and value are truly required ──────────────────
                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                {
                    return Json(new { success = false, message = "Label and Value are required." });
                }

                var newContact = new tbl_contact
                {
                    infoType = (infoType ?? "other").Trim(),
                    label = label.Trim(),
                    value = value.Trim(),
                    icon = (icon ?? "").Trim(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    isActive = true
                };

                db.tbl_contact.Add(newContact);
                db.SaveChanges();

                return Json(new { success = true, message = "Contact info created successfully.", contactID = newContact.contactID });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/UpdateContact/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateContact(int id, string infoType, string label, string value, string icon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                {
                    return Json(new { success = false, message = "Label and Value are required." });
                }

                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id);

                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact info not found." });
                }

                contact.infoType = (infoType ?? "other").Trim();
                contact.label = label.Trim();
                contact.value = value.Trim();
                contact.icon = (icon ?? "").Trim();
                contact.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "Contact info updated successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/DeleteContact/1 - Soft delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteContact(int id)
        {
            try
            {
                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id);

                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact info not found." });
                }

                contact.isActive = false;
                contact.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Contact info deleted successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/RestoreContact/1 - Restore soft-deleted contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestoreContact(int id)
        {
            try
            {
                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id && !c.isActive);

                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found or already active." });
                }

                contact.isActive = true;
                contact.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Contact info restored successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RestoreContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Contact/PermanentDeleteContact/1 - Hard delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PermanentDeleteContact(int id)
        {
            try
            {
                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id && !c.isActive);

                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found or is still active." });
                }

                db.tbl_contact.Remove(contact);
                db.SaveChanges();

                return Json(new { success = true, message = "Contact permanently deleted." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermanentDeleteContact Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}