using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class AdminController :  Controller
    {
 
        [HttpPost]
        public ActionResult SavePayment(string gcash, string bank)
        {
            // TEMP SAVE (you can replace with DB later)
            System.IO.File.WriteAllText(Server.MapPath("~/App_Data/payment.txt"),
                $"GCash:{gcash}\nBank:{bank}");

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult GetPaymentInfo()
        {
            var data = new
            {
                gcash = "09123456789",
                bank = "BDO - 1234567890"
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }



    }
}