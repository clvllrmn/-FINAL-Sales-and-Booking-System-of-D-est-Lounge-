using System;
using System.ComponentModel.DataAnnotations;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_sales
    {
        [Key]
        public int SaleId { get; set; }

        public int? BookingId { get; set; }
        public int? CustomerId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
