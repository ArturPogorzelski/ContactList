using AutoMapper;
using ContactList.Application.Commands.Contact;
using ContactList.Application.Interfaces;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
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
    public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IMapper _mapper;

        public CreateContactCommandHandler(
            IContactRepository contactRepository,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
            IMapper mapper)
        {
            _contactRepository = contactRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _mapper = mapper;
        }

        public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            //var contact = _mapper.Map<Contact>(request.Contact);
            
            
            //return await _contactRepository.AddAsync(contact);

            // Sprawdzenie, czy użytkownik istnieje
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }




            // Sprawdzenie, czy kategoria istnieje (oprócz kategorii "Other")
            if (request.CategoryId != 3 && await _categoryRepository.GetByIdAsync(request.CategoryId) == null)
            {
                throw new NotFoundException("Category not found");
            }

            // Sprawdzenie, czy podkategoria istnieje (jeśli podano)
            if (request.SubcategoryId.HasValue && await _subcategoryRepository.GetByIdAsync(request.SubcategoryId.Value) == null)
            {
                throw new NotFoundException("Subcategory not found");
            }


            // Mapowanie DTO na encję Contact
            var contact = _mapper.Map<Contact>(request);
            contact.UserId = request.UserId; // Ustawienie właściciela kontaktu

            // Jeśli kategoria jest "Other", ustawiamy podaną wartość CustomSubcategory
            if (request.CategoryId == 3)
            {
                contact.CustomSubcategory = request.CustomSubcategory;
            }

            if (request.CategoryId == 3 && !string.IsNullOrEmpty(request.CustomSubcategory) && await _subcategoryRepository.GetByNameAsync(request.CustomSubcategory) == null)
            {
                var subcategory = new Subcategory
                {
                    CategoryId = request.CategoryId,
                    Name = request.CustomSubcategory
                };
                await _subcategoryRepository.AddAsync(subcategory);
                contact.SubcategoryId = subcategory.SubcategoryId;
            }
            else if (request.CategoryId == 3 && request.SubcategoryId == null && !string.IsNullOrEmpty(request.CustomSubcategory) && await _subcategoryRepository.GetByNameAsync(request.CustomSubcategory) != null) 
            {
                var subCat = await _subcategoryRepository.GetByNameAsync(request.CustomSubcategory);
                request.SubcategoryId = subCat.SubcategoryId;
            }
            else if (request.CategoryId == 3 && request.SubcategoryId != null && !string.IsNullOrEmpty(request.CustomSubcategory) && await _subcategoryRepository.GetByNameAsync(request.CustomSubcategory) != null)
            {
                var subCat = await _subcategoryRepository.GetByNameAsync(request.CustomSubcategory);
                if (subCat.SubcategoryId == request.SubcategoryId) 
                {
                    contact.CustomSubcategory = string.Empty;
                }
                if (subCat.SubcategoryId != request.SubcategoryId)
                {
                    var subCat2 = await _subcategoryRepository.GetByIdAsync(request.SubcategoryId.Value);

                    if (subCat2 != null)
                    {
                        contact.CustomSubcategory = string.Empty;
                    }
                    else 
                    {
                        var subcategory = new Subcategory
                        {
                            CategoryId = request.CategoryId,
                            Name = request.CustomSubcategory
                        };
                        await _subcategoryRepository.AddAsync(subcategory);
                        contact.SubcategoryId = subcategory.SubcategoryId;
                    }
                    
                }
            }
            else
            {
                contact.CustomSubcategory = string.Empty;
            }


            // Dodanie kontaktu do bazy danych
            contact = await _contactRepository.AddAsync(contact);

            // Mapowanie encji Contact z powrotem na DTO
            return _mapper.Map<ContactDto>(contact);
        }
    }
}

