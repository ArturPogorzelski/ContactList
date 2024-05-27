using System.ComponentModel.DataAnnotations;

namespace ContactList.Core.Dtos
{
   
        public class LoginRequestDto 
        {
            [Required(ErrorMessage = "Adres email jest wymagany.")]
            [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane.")]
            public string Password { get; set; }
        }
   
}
