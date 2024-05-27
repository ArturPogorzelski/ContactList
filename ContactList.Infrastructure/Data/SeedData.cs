using ContactList.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Dane początkowe dla ról
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "User" },
                new Role { RoleId = 2, Name = "Admin" }
            );

            //// Utworzenie hasher'a haseł
            //var passwordHasher = new PasswordHasher<User>();
            //// Dane początkowe dla użytkowników (z hasłami zahaszowanymi)
            //modelBuilder.Entity<User>().HasData(
            //    new User
            //    {
            //        UserId = 1,
            //        FirstName = "Admin",
            //        LastName = "Adminowski",
            //        Email = "admin@example.com",
            //        PasswordHash = passwordHasher.HashPassword(null, "Admin123!"), // Hashowanie hasła
                   
            //    },
            //    new User
            //    {
            //        UserId = 2,
            //        FirstName = "Jan",
            //        LastName = "Kowalski",
            //        Email = "jan.kowalski@example.com",
            //        PasswordHash = passwordHasher.HashPassword(null, "User123!"), // Hashowanie hasła
                    
            //    }
            //);

            // Dane początkowe dla powiązań użytkowników z rolami
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 2 }, // Admin ma rolę Admin (RoleId = 2)
                new UserRole { UserId = 2, RoleId = 1 }  // User ma rolę User (RoleId = 1)
            );

            // Dane początkowe dla kategorii
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Business" },
                new Category { CategoryId = 2, Name = "Private" },
                new Category { CategoryId = 3, Name = "Other" }
            );

            // Dane początkowe dla podkategorii
            modelBuilder.Entity<Subcategory>().HasData(
                new Subcategory { SubcategoryId = 1, CategoryId = 1, Name = "Boss" },
                new Subcategory { SubcategoryId = 2, CategoryId = 1, Name = "Employee" },
                new Subcategory { SubcategoryId = 3, CategoryId = 1, Name = "Client" },
                new Subcategory { SubcategoryId = 4, CategoryId = 2, Name = "Family" },
                new Subcategory { SubcategoryId = 5, CategoryId = 2, Name = "Friend" }
            // Pamiętaj, że dla kategorii "Other" nie dodajemy podkategorii
            );
        }
        private static byte[] GenerateRandomSalt()
        {
            // Użyj klasy RandomNumberGenerator do generowania bezpiecznych losowych bajtów
            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[128 / 8]; // Długość solenia w bajtach (128 bitów)
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}
