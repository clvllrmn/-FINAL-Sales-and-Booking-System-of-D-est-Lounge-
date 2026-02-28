using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models
{
    public class tbl_contact
    {
        public int contactID { get; set; }
        public string infoType { get; set; }      
        public string label { get; set; }         
        public string value { get; set; }       
        public string icon { get; set; }          
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isActive { get; set; }
    }
}