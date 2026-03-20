using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_notifications
    {

        [Key]
        public int NotificationId { get; set; }
        public int CustomerId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}