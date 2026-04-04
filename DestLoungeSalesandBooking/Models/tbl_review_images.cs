using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_review_images
    {
        [Key]
        public int ImageId { get; set; }
        public int ReviewId { get; set; }
        public string ImageUrl { get; set; }

        // map to actual DB column name, e.g. "created_at"
        [Column("created_at")]
        public DateTime createdAt { get; set; }
    }
}