using ContactList.Core.Dtos;
using System.ComponentModel.DataAnnotations;

namespace ContactList.Core.Dtos
{
    public class RegisterRequestDto : UserDto
    {
       
            [Required(ErrorMessage = "Hasło jest wymagane.")]
            [MinLength(8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Hasło musi zawierać co najmniej jedną dużą literę, jedną małą literę, jedną cyfrę i jeden znak specjalny.")]
        public string Password { get; set; }

    }
}
