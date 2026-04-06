using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_admin_notifications
    {
        [Key]
        public int AdminNotificationId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}