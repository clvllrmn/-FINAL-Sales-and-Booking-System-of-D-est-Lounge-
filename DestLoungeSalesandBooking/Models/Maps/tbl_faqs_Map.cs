using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_faqs_Map : EntityTypeConfiguration<tbl_faqs>
    {
        public tbl_faqs_Map()
        {
            // Table name
            ToTable("tbl_faqs");

            // Primary Key
            HasKey(t => t.faqID);

            // Properties
            Property(t => t.faqID)
                .HasColumnName("faqID")
                .IsRequired();

            Property(t => t.question)
                .HasColumnName("question")
                .IsRequired()
                .HasMaxLength(500);

            Property(t => t.answer)
                .HasColumnName("answer")
                .IsRequired();

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