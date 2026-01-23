using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_users
    {
        public int userID { get; set; }
        public int roleID { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string coNum { get; set; }
        public string address { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}