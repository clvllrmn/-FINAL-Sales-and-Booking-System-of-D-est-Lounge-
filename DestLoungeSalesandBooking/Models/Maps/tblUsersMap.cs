using System.Data.Entity.ModelConfiguration;

namespace DestLoungeSalesandBooking.Models.Maps
{
    public class tblUsersMap : EntityTypeConfiguration<tbl_users>
    {
        public tblUsersMap()
        {
            ToTable("tbl_users");

            HasKey(t => t.userID);

            Property(t => t.userID)
                .HasColumnName("userID")
                .IsRequired();

            Property(t => t.roleID)
                .HasColumnName("roleID")
                .IsRequired();

            Property(t => t.firstname)
                .HasColumnName("firstname")
                .IsRequired()
                .HasMaxLength(50);

            Property(t => t.lastname)
                .HasColumnName("lastname")
                .IsRequired()
                .HasMaxLength(50);

            Property(t => t.email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(100);

            Property(t => t.password)
                .HasColumnName("password")
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.coNum)
                .HasColumnName("coNum")
                .IsRequired();

            Property(t => t.address)
                .HasColumnName("address")
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.googleSub)
                .HasColumnName("googleSub")
                .IsOptional()
                .HasMaxLength(100);

            Property(t => t.createdAt)
                .HasColumnName("createdAt")
                .IsRequired();

            Property(t => t.updatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();
        }
    }
}