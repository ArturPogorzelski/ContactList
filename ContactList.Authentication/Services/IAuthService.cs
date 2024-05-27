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
    Task<(bool isSuccess, string message)> RegisterUser(User user, string password, IEnumerable<string> roleNames);
    Task<User> GetUserByEmailAsync(string email);
    bool VerifyPassword(string actualPassword, string hashedPassword, byte[] salt);
}
}
