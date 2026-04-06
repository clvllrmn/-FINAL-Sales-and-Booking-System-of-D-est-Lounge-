using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_payment_settings
    {
        [Key]
        public int PaymentSettingId { get; set; }

        public string GCash { get; set; }
        public string Bank { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string QRCodePath { get; set; }
    }
}
