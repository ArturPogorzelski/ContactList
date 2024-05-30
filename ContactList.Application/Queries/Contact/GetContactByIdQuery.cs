using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Queries.Contact
{
    public class GetContactByIdQuery : IRequest<ContactDto>
    {
        public int ContactId { get; set; }
        public int UserId { get; set; }
    }
}
