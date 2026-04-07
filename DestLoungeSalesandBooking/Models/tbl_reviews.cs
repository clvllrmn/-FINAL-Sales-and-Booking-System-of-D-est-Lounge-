using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace DestLoungeSalesandBooking.Models
{
    public class tbl_reviews
    {
        [Key]
        public int ReviewId { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }

        [Column("createdAt")]
        public DateTime? CreatedAt { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }

        public bool Flagged { get; set; }
        public string FlagReason { get; set; }
        public string FlagNote { get; set; }
        public DateTime? FlaggedAt { get; set; }
    }
}