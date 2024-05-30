using AutoMapper;
using ContactList.Application.Queries.Contact;
using ContactList.Core.Dtos;
using ContactList.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Handlers
{
    public class GetAllContactsForUserQueryHandler : IRequestHandler<GetAllContactsForUserQuery, IEnumerable<ContactDto>>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;

        public GetAllContactsForUserQueryHandler(IContactRepository contactRepository, IMapper mapper)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContactDto>> Handle(GetAllContactsForUserQuery request, CancellationToken cancellationToken)
        {
            var contacts = await _contactRepository.GetAllForUserAsync(request.UserId);
            return _mapper.Map<IEnumerable<ContactDto>>(contacts);
        }
    }
}
