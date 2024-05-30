using AutoMapper;
using ContactList.Application.Commands.Contact;
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
    public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, ContactDto>
    {
        private readonly IContactRepository _contactRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IMapper _mapper;

        public UpdateContactCommandHandler(
            IContactRepository contactRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
            IMapper mapper)
        {
            _contactRepository = contactRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _mapper = mapper;
        }

        public async Task<ContactDto> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await _contactRepository.GetByIdAsync(request.ContactId);
            if (contact == null)
            {
                throw new NotFoundException("Contact not found");
            }

            if (contact.UserId != request.UserId)
            {
                throw new ContactList.Core.Exceptions.UnauthorizedAccessException("Brak uprawnień do edycji tego kontaktu.");
            }

            _mapper.Map(request, contact); // Aktualizacja właściwości kontaktu

            await _contactRepository.UpdateAsync(contact);

            return _mapper.Map<ContactDto>(contact);
        }
    }
}
