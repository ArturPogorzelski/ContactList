using ContactList.Application.Commands;
using ContactList.Application.Queries;
using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(RegisterRequestDto registerRequestDto);
        Task<string> AuthenticateUserAsync(LoginRequestDto loginRequestDto); 
        Task<UserDto> GetUserByIdAsync(int id);
    }
}
