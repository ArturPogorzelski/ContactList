using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Commands.Contact
{
    public class UpdateContactCommand : IRequest<ContactDto>
    {
        //public CreateContactRequestDto Contact { get; set; }
        public int ContactId { get; set; }
        public int UserId { get; set; }
    }
}
