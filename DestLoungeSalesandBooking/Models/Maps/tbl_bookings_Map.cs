using System.Data.Entity.ModelConfiguration;
using DestLoungeSalesandBooking.Models;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_bookings_Map : EntityTypeConfiguration<tbl_bookings>
    {
        public tbl_bookings_Map()
        {
            ToTable("tbl_bookings");
            HasKey(b => b.BookingId);

            Property(b => b.Status).HasMaxLength(20);
            Property(b => b.Notes).HasMaxLength(255);
        }
    }
}
