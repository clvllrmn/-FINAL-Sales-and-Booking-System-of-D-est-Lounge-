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

        public DestLoungeSalesandBookingContext()
    : base("Server=127.0.0.1;Port=3306;Database=destlounge_db;Uid=root;Pwd=;")
        {
        }

        // DbSets
        public virtual DbSet<tbl_users> tbl_users { get; set; }
        public virtual DbSet<tbl_faqs> tbl_faqs { get; set; }
        public virtual DbSet<tbl_sales> tbl_sales { get; set; }
        public virtual DbSet<tbl_sale_items> tbl_sale_items { get; set; }
        public virtual DbSet<tbl_bookings> tbl_bookings { get; set; }
        public virtual DbSet<tbl_contact> tbl_contact { get; set; } 
        public virtual DbSet<tbl_homepage_content> tbl_homepage_content { get; set; }

        public virtual DbSet<tbl_services> tbl_services { get; set; }

        public DbSet<tbl_notifications> tbl_notifications { get; set; }
        public DbSet<tbl_review_requests> tbl_review_requests { get; set; }

        public DbSet<tbl_reviews> tbl_reviews { get; set; }
        public DbSet<tbl_review_images> tbl_review_images { get; set; }

        public DbSet<tbl_admin_notifications> tbl_admin_notifications { get; set; }

        public System.Data.Entity.DbSet<tbl_payment_settings> tbl_payment_settings { get; set; }






        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Apply configurations
            modelBuilder.Configurations.Add(new tblUsersMap());
            modelBuilder.Configurations.Add(new tbl_faqs_Map());
            modelBuilder.Configurations.Add(new tbl_bookings_Map());
            modelBuilder.Configurations.Add(new tbl_contact_Map());  
            modelBuilder.Configurations.Add(new tbl_homepage_content_Map());
            modelBuilder.Configurations.Add(new tbl_services_Map());


            base.OnModelCreating(modelBuilder);
        }
    }
}