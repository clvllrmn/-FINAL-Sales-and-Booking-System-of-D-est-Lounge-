using System.ComponentModel.DataAnnotations;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_sale_items
    {
        [Key]
        public int SaleItemId { get; set; }

        public int SaleId { get; set; }

        public string ItemType { get; set; }   // "Service" or "Product"
        public int ItemId { get; set; }
        public string ItemName { get; set; }

        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
