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
            Property(b => b.ReminderSent)
                .HasColumnName("ReminderSent")
                .IsRequired();
            Property(b => b.Reminder3HoursSent)
                .HasColumnName("Reminder3HoursSent")
                .IsRequired();

            // ADD THESE TWO:
            Property(b => b.NailTech)
                .HasColumnName("NailTech")
                .HasMaxLength(100)
                .IsOptional();
            Property(b => b.ReferenceNo)
                .HasColumnName("ReferenceNo")
                .HasMaxLength(50)
                .IsOptional();
        }
    }
}