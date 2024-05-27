using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using ContactList.Infrastructure.Helpers.Strategy;
using ContactList.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using ContactList.Infrastructure;
using ContactList.API.Controllers.v1;
using ContactList.API.Configuration;
using Microsoft.Extensions.Options;

namespace ContactList.API.Services
{
       public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IMapper _mapper;
        private readonly IRetryStrategy _retryStrategy;
        private readonly IRetryHelper _retryHelper;
        private readonly RetryPolicyConfig _retryPolicyConfig;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IContactRepository contactRepository,
            IMapper mapper,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
             ILogger<ContactService> logger,
             IOptions<RetryPolicyConfig> retryPolicyConfig = null,
            IRetryHelper retryHelper = null, 
            IRetryStrategy retryStrategy = null
             )
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _logger = logger;
            _retryPolicyConfig = retryPolicyConfig.Value;
            _retryHelper = retryHelper;

            if (retryStrategy != null)
            {
                _retryStrategy = retryStrategy;
            }
            else if (_retryHelper != null)
            {
                _retryStrategy = new RetryHelperStrategy(_retryHelper);
            }
            else
            {
                _retryStrategy = new RetryHelperStaticStrategy();
            }
        }
        //public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        //{
        //    var contacts = await _contactRepository.GetAllAsync();
        //    return _mapper.Map<IEnumerable<ContactDto>>(contacts);
        //}
        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            return await _retryStrategy.ExecuteWithRetriesAsync(async () =>
            {
                var contacts = await _contactRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ContactDto>>(contacts);
            }, nameof(GetAllContactsAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay); 
        }
        //public async Task<IEnumerable<ContactDto>> GetAllContactsForUserAsync(int userId)
        //{
        //    var contacts = await _contactRepository.GetAllForUserAsync(userId);
        //    return _mapper.Map<IEnumerable<ContactDto>>(contacts);
        //}
        public async Task<IEnumerable<ContactDto>> GetAllContactsForUserAsync(int userId)
        {
            return await _retryStrategy.ExecuteWithRetriesAsync(async () =>
            {
                var contacts = await _contactRepository.GetAllForUserAsync(userId);
                return _mapper.Map<IEnumerable<ContactDto>>(contacts);
            }, nameof(GetAllContactsForUserAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
        }

        //public async Task<ContactDto> GetContactByIdAsync(int id, int userId)
        //{
        //    var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
        //    if (contact == null)
        //    {
        //        throw new NotFoundException("Contact not found");
        //    }
        //    return _mapper.Map<ContactDto>(contact);
        //}
        public async Task<ContactDto> GetContactByIdAsync(int id, int userId)
        {
            return await _retryStrategy.ExecuteWithRetriesAsync(async () =>
            {
                var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
                if (contact == null)
                {
                    throw new NotFoundException("Contact not found");
                }
                return _mapper.Map<ContactDto>(contact);
            }, nameof(GetContactByIdAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
        }


        //public async Task<ContactDto> CreateContactAsync(CreateContactRequestDto requestDto, int userId)
        //{
        //    // Sprawdzenie, czy użytkownik istnieje
        //    var user = await _userRepository.GetByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new NotFoundException("User not found");
        //    }

        //    // Sprawdzenie, czy kategoria istnieje (oprócz kategorii "Other")
        //    if (requestDto.CategoryId != 3 && await _categoryRepository.GetByIdAsync(requestDto.CategoryId) == null)
        //    {
        //        throw new NotFoundException("Category not found");
        //    }

        //    // Sprawdzenie, czy podkategoria istnieje (jeśli podano)
        //    if (requestDto.SubcategoryId.HasValue && await _subcategoryRepository.GetByIdAsync(requestDto.SubcategoryId.Value) == null)
        //    {
        //        throw new NotFoundException("Subcategory not found");
        //    }

        //    // Mapowanie DTO na encję Contact
        //    var contact = _mapper.Map<Contact>(requestDto);
        //    contact.UserId = userId; // Ustawienie właściciela kontaktu

        //    // Jeśli kategoria jest "Other", ustawiamy podaną wartość CustomSubcategory
        //    if (requestDto.CategoryId == 3)
        //    {
        //        contact.CustomSubcategory = requestDto.CustomSubcategory;
        //    }

        //    // Dodanie kontaktu do bazy danych
        //    contact = await _contactRepository.AddAsync(contact);

        //    // Mapowanie encji Contact z powrotem na DTO
        //    return _mapper.Map<ContactDto>(contact);
        //}
        public async Task<ContactDto> CreateContactAsync(CreateContactRequestDto requestDto, int userId)
        {
            try
            {
                // Sprawdzenie, czy użytkownik istnieje
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }

                // Sprawdzenie, czy kategoria istnieje (oprócz kategorii "Other")
                if (requestDto.CategoryId != 3 && await _categoryRepository.GetByIdAsync(requestDto.CategoryId) == null)
                {
                    throw new NotFoundException("Category not found");
                }

                // Sprawdzenie, czy podkategoria istnieje (jeśli podano)
                if (requestDto.SubcategoryId.HasValue && await _subcategoryRepository.GetByIdAsync(requestDto.SubcategoryId.Value) == null)
                {
                    throw new NotFoundException("Subcategory not found");
                }

                // Mapowanie DTO na encję Contact
                var contact = _mapper.Map<Contact>(requestDto);
                contact.UserId = userId;
                if (requestDto.CategoryId == 3)
                {
                    contact.CustomSubcategory = requestDto.CustomSubcategory;
                }
                else 
                {
                    contact.CustomSubcategory = string.Empty;
                }

                contact = await _retryStrategy.ExecuteWithRetriesAsync(async () => await _contactRepository.AddAsync(contact), nameof(CreateContactAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
                return _mapper.Map<ContactDto>(contact);
            }
            catch (DbUpdateException ex) // Przechwytujemy wyjątek związany z bazą danych
            {
                _logger.LogError(ex, "Błąd bazy danych podczas tworzenia kontaktu: {ErrorMessage}", ex.InnerException?.Message); // Dodajemy szczegółowy komunikat błędu
                throw new DataException("Błąd bazy danych podczas tworzenia kontaktu.", ex);
            }
        }

        

        public async Task UpdateContactAsync(int id, UpdateContactRequestDto requestDto, int userId)
        {
            try
            {
                var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
                if (contact == null)
                {
                    throw new NotFoundException("Contact not found");
                }

                // Sprawdzenie, czy kategoria istnieje (oprócz kategorii "Other")
                if (requestDto.CategoryId != 3 && await _categoryRepository.GetByIdAsync(requestDto.CategoryId) == null)
                {
                    throw new NotFoundException("Category not found");
                }

                // Sprawdzenie, czy podkategoria istnieje (jeśli podano)
                if (requestDto.SubcategoryId.HasValue && await _subcategoryRepository.GetByIdAsync(requestDto.SubcategoryId.Value) == null)
                {
                    throw new NotFoundException("Subcategory not found");
                }
                _mapper.Map(requestDto, contact);
                if (requestDto.CategoryId == 3)
                {
                    contact.CustomSubcategory = requestDto.CustomSubcategory;
                }
                else
                {
                    contact.CustomSubcategory = string.Empty;
                }

                await _retryStrategy.ExecuteWithRetriesAsync(async () => await _contactRepository.UpdateAsync(contact), nameof(UpdateContactAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Błąd bazy danych podczas aktualizacji kontaktu: {ErrorMessage}", ex.InnerException?.Message); // Dodajemy szczegółowy komunikat błędu
                throw new DataException("Błąd bazy danych podczas aktualizacji kontaktu.", ex);
            }
        }

        //public async Task DeleteContactAsync(int id, int userId)
        //{
        //    var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
        //    if (contact == null)
        //    {
        //        throw new NotFoundException("Contact not found");
        //    }

        //    await _contactRepository.DeleteAsync(id);
        //}
        public async Task DeleteContactAsync(int id, int userId)
        {
            var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
            if (contact == null)
            {
                throw new NotFoundException("Contact not found");
            }

            await _retryStrategy.ExecuteWithRetriesAsync(async () => await _contactRepository.DeleteAsync(id), nameof(DeleteContactAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
        }
    }
}
