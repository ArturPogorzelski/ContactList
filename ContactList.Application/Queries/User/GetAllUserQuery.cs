using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Queries
{
    public class GetAllUserQuery : IRequest<IEnumerable<UserDto>>
    {
    }
}
