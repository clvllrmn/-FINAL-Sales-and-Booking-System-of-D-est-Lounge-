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
       /* public virtual DbSet<tbl_bookings> tbl_bookings { get; set; }
        public virtual DbSet<tbl_services> tbl_services { get; set; }
        public virtual DbSet<tbl_booking_services> tbl_booking_services { get; set; }
        public virtual DbSet<tbl_nail_techs> tbl_nail_techs { get; set; }*/

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Apply configurations
            modelBuilder.Configurations.Add(new tblUsersMap());
          /*  modelBuilder.Configurations.Add(new tbl_bookings_Map());
            modelBuilder.Configurations.Add(new tbl_services_Map());
            modelBuilder.Configurations.Add(new tbl_booking_services_Map());
            modelBuilder.Configurations.Add(new tbl_nail_techs_Map());*/

            base.OnModelCreating(modelBuilder);
        }
    }
}