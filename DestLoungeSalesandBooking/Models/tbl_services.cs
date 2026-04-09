using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace DestLoungeSalesandBooking.Models
{
    public class tbl_services
    {
        public int service_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }  // ← removed [StringLength(500)]
        public decimal price { get; set; }
        public sbyte is_active { get; set; }
        public string category { get; set; }
    }
}