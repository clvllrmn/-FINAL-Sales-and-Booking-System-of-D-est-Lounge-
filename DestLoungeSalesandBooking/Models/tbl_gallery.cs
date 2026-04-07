using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_gallery
    {
        public int galleryId { get; set; }
        public string caption { get; set; }
        public string description { get; set; }
        public string imageUrl { get; set; }
        public string fileName { get; set; }
        public long fileSizeBytes { get; set; }
        public bool isActive { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}