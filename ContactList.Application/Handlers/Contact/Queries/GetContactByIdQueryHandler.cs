using AutoMapper;
using ContactList.Application.Queries.Contact;
using ContactList.Core.Dtos;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Handlers
{
    public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;

        public GetContactByIdQueryHandler(IContactRepository contactRepository, IMapper mapper)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
        }

        public async Task<ContactDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
        {
            var contact = await _contactRepository.GetByIdForUserAsync(request.ContactId, request.UserId);
            if (contact == null)
            {
                throw new NotFoundException("Contact not found");
            }
            return _mapper.Map<ContactDto>(contact);
        }
    }
}
