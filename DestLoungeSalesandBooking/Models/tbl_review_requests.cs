using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace DestLoungeSalesandBooking.Models
{
    public class tbl_review_requests
    {

        [Key]
        public int ReviewRequestId { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}