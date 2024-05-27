using ContactList.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(RegisterRequestDto registerRequestDto); // rejestracja
        Task<string> AuthenticateUserAsync(LoginRequestDto loginRequestDto); // logowanie
        Task<UserDto> GetUserByIdAsync(int id); // pobranie użytkownika po ID
        Task<UserDto> GetUserByEmailAsync(string email); 
        
    }
}
