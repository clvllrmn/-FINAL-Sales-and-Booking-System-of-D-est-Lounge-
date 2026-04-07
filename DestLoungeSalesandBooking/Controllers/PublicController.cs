using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Linq;        // ← this is the missing one
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class PublicContactController : Controller
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
                    .Select(c => new {
                        contactID = c.contactID,
                        infoType = c.infoType,
                        label = c.label,
                        value = c.value,
                        icon = c.icon
                    }).ToList();
                return Json(new { success = true, data = contacts },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}