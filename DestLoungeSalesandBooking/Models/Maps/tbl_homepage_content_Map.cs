using System.Data.Entity.ModelConfiguration;
using DestLoungeSalesandBooking.Models;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tbl_homepage_content_Map : EntityTypeConfiguration<tbl_homepage_content>
    {
        public tbl_homepage_content_Map()
        {
            ToTable("tbl_homepage_content");
            HasKey(t => t.contentID);

            Property(t => t.contentID).HasColumnName("contentID").IsRequired();
            Property(t => t.contentType).HasColumnName("contentType").IsRequired().HasMaxLength(50);
            Property(t => t.contentValue).HasColumnName("contentValue").IsRequired().HasMaxLength(1000);
            Property(t => t.createdAt).HasColumnName("createdAt").IsRequired();
            Property(t => t.updatedAt).HasColumnName("updatedAt").IsRequired();
            Property(t => t.isActive).HasColumnName("isActive").IsRequired();
        }
    }
}