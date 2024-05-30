using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Commands
{

    public class LoginUserCommand : IRequest<LoginRequestDto>
    {
        //public LoginRequestDto User { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
