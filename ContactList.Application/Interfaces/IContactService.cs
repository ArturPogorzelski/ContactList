using ContactList.Application.Commands.Contact;
using ContactList.Application.Queries.Contact;
using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Interfaces
{
    public interface IContactService
    {
        Task<IRequestHandler<GetAllContactsForUserQuery, IEnumerable<ContactDto>>> GetAllContactsForUserAsync();
        Task<IRequestHandler<GetContactByIdQuery, ContactDto>> GetContactByIdAsync();
        Task<IRequestHandler<CreateContactCommand, ContactDto>> CreateContactAsync();
        Task<IRequestHandler<UpdateContactCommand, ContactDto>> UpdateContactAsync();
        Task<IRequestHandler<DeleteContactCommand, Unit>> DeleteContactAsync();
    }
}
