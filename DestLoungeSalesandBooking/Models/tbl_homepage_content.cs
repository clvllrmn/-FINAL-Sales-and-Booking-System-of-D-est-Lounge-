using System;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_homepage_content
    {
        public int contentID { get; set; }
        public string contentType { get; set; }
        public string contentValue { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isActive { get; set; }
    }
}