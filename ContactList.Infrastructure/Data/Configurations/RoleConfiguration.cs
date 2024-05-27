using ContactList.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles"); // Nazwa tabeli w bazie danych
            builder.HasKey(r => r.RoleId); // Primary key

            builder.Property(r => r.Name)
                .IsRequired() // Pole wymagane
                .HasMaxLength(50); // Maksymalna długość nazwy roli

            // Dodatkowe konfiguracje, np. relacje z innymi tabelami
            builder.HasMany(r => r.UserRoles)
                   .WithOne(ur => ur.Role)
                   .HasForeignKey(ur => ur.RoleId);

            builder.HasData(
               new Role { RoleId = 1, Name = "user" },
               new Role { RoleId = 2, Name = "admin" },
               new Role { RoleId = 3, Name = "manager" }
           );
        }
    }
}
