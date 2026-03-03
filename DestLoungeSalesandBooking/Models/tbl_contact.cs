using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_contact
    {
        public int contactID { get; set; }
        public string infoType { get; set; }      // address, hours, phone, email, social, other
        public string label { get; set; }         // "Find us at", "Call us", "Email us", etc
        public string value { get; set; }         // The actual contact info
        public string icon { get; set; }          // Font Awesome icon class
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isActive { get; set; }
    }
}