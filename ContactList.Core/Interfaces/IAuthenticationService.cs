using ContactList.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> AuthenticateAsync(LoginRequestDto loginRequestDto);
        string GenerateToken(UserDto userDto);
    }
}
