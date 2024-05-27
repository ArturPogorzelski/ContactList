using ContactList.Core.Dtos;
using System.ComponentModel.DataAnnotations;

namespace ContactList.Core.Dtos
{
    public class RegisterRequestDto : UserDto
    {
       
            [Required(ErrorMessage = "Hasło jest wymagane.")]
            [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
            public string Password { get; set; }
       
    }
}
