using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_gallery_Map : EntityTypeConfiguration<tbl_gallery>
    {
        public tbl_gallery_Map()
        {
            ToTable("tbl_gallery");

            HasKey(t => t.galleryId);

            Property(t => t.galleryId)
                .HasColumnName("galleryId")
                .IsRequired();

            Property(t => t.caption)
                .HasColumnName("caption")
                .IsRequired()
                .HasMaxLength(300);

            Property(t => t.imageUrl)
                .HasColumnName("imageUrl")
                .IsRequired()
                .HasMaxLength(500);

            Property(t => t.fileName)
                .HasColumnName("fileName")
                .IsRequired()
                .HasMaxLength(260);

            Property(t => t.fileSizeBytes)
                .HasColumnName("fileSizeBytes")
                .IsRequired();

            Property(t => t.isActive)
                .HasColumnName("isActive")
                .IsRequired();

            Property(t => t.createdAt)
                .HasColumnName("createdAt")
                .IsRequired();

            Property(t => t.updatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();
        }
    }
}