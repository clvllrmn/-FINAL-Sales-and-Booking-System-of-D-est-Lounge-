    using System;
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using System.Web;

    namespace DestLoungeSalesandBooking.Models.Maps
    {
        public class tblUsersMap : EntityTypeConfiguration<tbl_users>
        {
            public tblUsersMap()
            {
                // Table name
                ToTable("tbl_users");
                // Primary Key
                HasKey(t => t.userID);
                // Properties
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
                    .HasMaxLength(200);
                Property(t => t.coNum)
                    .HasColumnName("coNum")
                    .IsRequired();
                Property(t => t.address)
                    .HasColumnName("address")
                    .IsRequired()
                    .HasMaxLength(255);
                Property(t => t.createdAt)
                    .HasColumnName("createdAt")
                    .IsRequired();
                Property(t => t.updatedAt)
                    .HasColumnName("updatedAt")
                    .IsRequired();
            }
        }
    }