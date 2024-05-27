using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class CreateContactRequestDto
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(100, ErrorMessage = "Imię nie może przekraczać 100 znaków.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100, ErrorMessage = "Nazwisko nie może przekraczać 100 znaków.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Adres email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        public int CategoryId { get; set; }

        public int? SubcategoryId { get; set; } // Może być null

        [MaxLength(100, ErrorMessage = "Podkategoria nie może przekraczać 100 znaków.")]
        public string? CustomSubcategory { get; set; } // Może być null

        public DateTime? DateOfBirth { get; set; }
    }
}
