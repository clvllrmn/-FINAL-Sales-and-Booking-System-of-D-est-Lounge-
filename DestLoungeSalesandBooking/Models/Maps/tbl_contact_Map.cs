using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_contact_Map : EntityTypeConfiguration<tbl_contact>
    {
        public tbl_contact_Map()
        {
            // Table name
            ToTable("tbl_contact");

            // Primary Key
            HasKey(c => c.contactID);

            // Properties
            Property(c => c.contactID)
                .HasColumnName("contactID")
                .IsRequired();

            Property(c => c.infoType)
                .HasColumnName("infoType")
                .IsRequired()
                .HasMaxLength(50);

            Property(c => c.label)
                .HasColumnName("label")
                .IsRequired()
                .HasMaxLength(100);

            Property(c => c.value)
                .HasColumnName("value")
                .IsRequired();

            Property(c => c.icon)
                .HasColumnName("icon")
                .HasMaxLength(100);

            Property(c => c.createdAt)
                .HasColumnName("createdAt")
                .IsRequired();

            Property(c => c.updatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            Property(c => c.isActive)
                .HasColumnName("isActive")
                .IsRequired();
        }
    }
}