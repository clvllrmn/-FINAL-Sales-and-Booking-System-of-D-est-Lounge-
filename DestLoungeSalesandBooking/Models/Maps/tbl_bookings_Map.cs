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

            Property(b => b.BookingId)
                .HasColumnName("BookingID");

            Property(b => b.CustomerId)
                .HasColumnName("CustomerID")
                .IsRequired();

            Property(b => b.ServiceId)
                .HasColumnName("ServiceID")
                .IsRequired();

            Property(b => b.BookingDate)
                .HasColumnName("BookingDate")
                .IsRequired();

            Property(b => b.StartTime)
                .HasColumnName("StartTime")
                .IsRequired();

            Property(b => b.EndTime)
                .HasColumnName("EndTime")
                .IsRequired();

            Property(b => b.Status)
                .HasColumnName("Status")
                .HasMaxLength(20)
                .IsRequired();

            Property(b => b.Notes)
                .HasColumnName("Notes")
                .HasMaxLength(255)
                .IsOptional();

            Property(b => b.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();
        }
    }
}