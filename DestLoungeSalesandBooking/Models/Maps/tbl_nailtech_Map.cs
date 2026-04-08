using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

    namespace DestLoungeSalesandBooking.Models.Maps
    {
        public class tbl_nailtech_Map : EntityTypeConfiguration<tbl_nailtech>
        {
            public tbl_nailtech_Map()
            {
                ToTable("tbl_nailtech");
                HasKey(t => t.nailTechId);
                Property(t => t.nailTechId).HasColumnName("nailTechId").IsRequired();
                Property(t => t.name).HasColumnName("name").IsRequired().HasMaxLength(200);
                Property(t => t.specialization).HasColumnName("specialization").HasMaxLength(200);
                Property(t => t.contact).HasColumnName("contact").HasMaxLength(50);
                Property(t => t.status).HasColumnName("status").IsRequired().HasMaxLength(20);
                Property(t => t.notes).HasColumnName("notes");
                Property(t => t.createdAt).HasColumnName("createdAt").IsRequired();
                Property(t => t.updatedAt).HasColumnName("updatedAt").IsRequired();
                Property(t => t.isDeleted).HasColumnName("isDeleted").IsRequired();
            }
        }
    }