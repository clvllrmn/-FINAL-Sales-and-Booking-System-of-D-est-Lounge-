using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_nailtech
    {
        public int nailTechId { get; set; }
        public string name { get; set; }
        public string specialization { get; set; }
        public string contact { get; set; }
        public string status { get; set; }
        public string notes { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isDeleted { get; set; }
    }
}