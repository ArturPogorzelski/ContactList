using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Authentication.Services
{
   public interface IAuthService
{
        Task<string> GenerateJwtToken(User user);
        Task<bool> ValidateUser(string email, string password);
        Task<(bool isValid, string error)> RegisterUser(User user, string password, IEnumerable<string> roleNames);
        (string Hash, byte[] Salt) HashPassword(string password);
        bool VerifyPassword(string actualPassword, string hashedPassword, byte[] salt);
        Task<User> GetUserByEmailAsync(string email); // Nowa metoda
        Task<string> AuthenticateUserAsync(LoginRequestDto loginRequestDto);
    }
}
