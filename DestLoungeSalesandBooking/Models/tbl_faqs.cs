using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_faqs
    {
        public int faqID { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isActive { get; set; }
    }
}