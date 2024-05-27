using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class ContactDto
    {
        public int ContactId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // Dodajemy nazwę kategorii dla wygody

        public int? SubcategoryId { get; set; } // Może być null
        public string? SubcategoryName { get; set; } // Dodajemy nazwę podkategorii dla wygody
        public string? CustomSubcategory { get; set; } // Może być null
    }
}
