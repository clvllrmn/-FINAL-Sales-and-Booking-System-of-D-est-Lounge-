using System;
using System.ComponentModel.DataAnnotations;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_bookings
    {
        [Key]
        public int BookingId { get; set; }

        public int CustomerId { get; set; }
        public int ServiceId { get; set; }

        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
