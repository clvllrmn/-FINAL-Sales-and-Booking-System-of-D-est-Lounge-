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
            HasKey(t => t.contactID);

            // Properties
            Property(t => t.contactID)
                .HasColumnName("contactID")
                .IsRequired();

            Property(t => t.infoType)
                .HasColumnName("infoType")
                .IsRequired()
                .HasMaxLength(50);

            Property(t => t.label)
                .HasColumnName("label")
                .IsRequired()
                .HasMaxLength(100);

            Property(t => t.value)
                .HasColumnName("value")
                .IsRequired();

            Property(t => t.icon)
                .HasColumnName("icon")
                .HasMaxLength(100);

            Property(t => t.createdAt)
                .HasColumnName("createdAt")
                .IsRequired();

            Property(t => t.updatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            Property(t => t.isActive)
                .HasColumnName("isActive")
                .IsRequired();
        }
    }
}