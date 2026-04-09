using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_services_Map : EntityTypeConfiguration<DestLoungeSalesandBooking.Models.tbl_services>
    {
        public tbl_services_Map()
        {
            ToTable("tbl_services");

            HasKey(t => t.service_id);

            Property(t => t.service_id)
                .HasColumnName("service_id");

            Property(t => t.name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            Property(t => t.description)
     .HasColumnName("description")
     .IsOptional();  
            Property(t => t.price)
                .HasColumnName("price")
                .HasPrecision(10, 2)
                .IsRequired();

            Property(t => t.is_active)
                .HasColumnName("is_active")
                .IsRequired();

            Property(t => t.category)
                .HasColumnName("category")
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}