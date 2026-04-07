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
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateContact(string infoType, string label, string value, string icon)
        {
            try
            {
                infoType = (infoType ?? "other").Trim().ToLower();
                label = (label ?? "").Trim();
                value = (value ?? "").Trim();
                icon = (icon ?? "").Trim();

                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                {
                    return Json(new { success = false, message = "Label and Value are required." });
                }

                if (infoType == "phone")
                {
                    value = new string(value.Where(char.IsDigit).ToArray());

                    if (value.Length != 11)
                    {
                        return Json(new { success = false, message = "Contact number must be exactly 11 digits." });
                    }
                }

                var existingType = db.tbl_contact.FirstOrDefault(c => c.infoType == infoType && c.isActive);
                if (existingType != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "A contact for this type already exists. Please edit the existing one instead."
                    });
                }

                var newContact = new tbl_contact
                {
                    infoType = infoType,
                    label = label,
                    value = value,
                    icon = icon,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    isActive = true
                };

                db.tbl_contact.Add(newContact);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Contact info created successfully.",
                    contactID = newContact.contactID
                });
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException != null
                    ? (ex.InnerException.InnerException != null
                        ? ex.InnerException.InnerException.Message
                        : ex.InnerException.Message)
                    : ex.Message;

                return Json(new { success = false, message = realError });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateContact(int id, string infoType, string label, string value, string icon)
        {
            try
            {
                infoType = (infoType ?? "other").Trim().ToLower();
                label = (label ?? "").Trim();
                value = (value ?? "").Trim();
                icon = (icon ?? "").Trim();

                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                {
                    return Json(new { success = false, message = "Label and Value are required." });
                }

                if (infoType == "phone")
                {
                    value = new string(value.Where(char.IsDigit).ToArray());

                    if (value.Length != 11)
                    {
                        return Json(new { success = false, message = "Contact number must be exactly 11 digits." });
                    }
                }

                var contact = db.tbl_contact.FirstOrDefault(c => c.contactID == id);

                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact info not found." });
                }

                contact.infoType = infoType;
                contact.label = label;
                contact.value = value;
                contact.icon = icon;
                contact.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "Contact info updated successfully." });
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException != null
                    ? (ex.InnerException.InnerException != null
                        ? ex.InnerException.InnerException.Message
                        : ex.InnerException.Message)
                    : ex.Message;

                System.Diagnostics.Debug.WriteLine("UpdateContact Error: " + realError);
                return Json(new { success = false, message = realError });
            }
        }

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
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestoreContact(int id)
        {
            try
            {
                var contact = db.tbl_contact
                    .FirstOrDefault(c => c.contactID == id && !c.isActive);

                if (contact == null)
                    return Json(new
                    {
                        success = false,
                        message = "Contact not found or already active."
                    });

                // Check if an active contact of the same type already exists
                bool typeConflict = db.tbl_contact
                    .Any(c => c.infoType == contact.infoType
                           && c.isActive
                           && c.contactID != id);

                if (typeConflict)
                    return Json(new
                    {
                        success = false,
                        message = "An active contact of type '"
                                                + contact.infoType
                                                + "' already exists. Delete or edit it first."
                    });

                contact.isActive = true;
                contact.updatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Contact info restored successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

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
