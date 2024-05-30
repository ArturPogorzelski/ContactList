using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Commands
{
    public class RegisterUserCommand : IRequest<UserDto>
    {
        public RegisterRequestDto User { get; set; }
    }
}
