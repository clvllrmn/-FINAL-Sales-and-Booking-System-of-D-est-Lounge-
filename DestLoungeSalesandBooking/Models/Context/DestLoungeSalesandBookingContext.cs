using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using DestLoungeSalesandBooking.Models.Maps;

namespace DestLoungeSalesandBooking.Models.Context
{
    public class DestLoungeSalesandBookingContext : DbContext
    {
        static DestLoungeSalesandBookingContext()
        {
            Database.SetInitializer<DestLoungeSalesandBookingContext>(null);
        }

        public DestLoungeSalesandBookingContext() : base("Name=db_destloungesaleandbooking") { }

        // DbSets
        public virtual DbSet<tbl_users> tbl_users { get; set; }
        public virtual DbSet<tbl_faqs> tbl_faqs { get; set; }
        public virtual DbSet<tbl_sales> tbl_sales { get; set; }
        public virtual DbSet<tbl_sale_items> tbl_sale_items { get; set; }
        public virtual DbSet<tbl_bookings> tbl_bookings { get; set; }
        public virtual DbSet<tbl_contact> tbl_contact { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Apply configurations
            modelBuilder.Configurations.Add(new tblUsersMap());
            modelBuilder.Configurations.Add(new tbl_faqs_Map());
            modelBuilder.Configurations.Add(new tbl_bookings_Map());
            modelBuilder.Configurations.Add(new tbl_contact_Map());

            base.OnModelCreating(modelBuilder);
        }
    }
}