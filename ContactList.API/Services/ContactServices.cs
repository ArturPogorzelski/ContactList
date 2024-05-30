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
        // Wstrzyknięte zależności
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IMapper _mapper;
        private readonly IRetryStrategy _retryStrategy;
        private readonly IRetryHelper _retryHelper;
        private readonly RetryPolicyConfig _retryPolicyConfig;
        private readonly ILogger<ContactService> _logger;

        // Konstruktor inicjalizujący wszystkie zależności
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

            // Konfiguracja strategii ponawiania
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

        // Pobranie wszystkich kontaktów z zastosowaniem polityki ponawiania
        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            return await _retryStrategy.ExecuteWithRetriesAsync(async () =>
            {
                var contacts = await _contactRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ContactDto>>(contacts);
            }, nameof(GetAllContactsAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
        }

        // Pobranie kontaktów dla konkretnego użytkownika z zastosowaniem polityki ponawiania
        public async Task<IEnumerable<ContactDto>> GetAllContactsForUserAsync(int userId)
        {
            return await _retryStrategy.ExecuteWithRetriesAsync(async () =>
            {
                var contacts = await _contactRepository.GetAllForUserAsync(userId);
                return _mapper.Map<IEnumerable<ContactDto>>(contacts);
            }, nameof(GetAllContactsForUserAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
        }

        // Pobranie szczegółów konkretnego kontaktu z zastosowaniem polityki ponawiania
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

        // Tworzenie nowego kontaktu z odpowiednimi weryfikacjami i mapowaniem
        public async Task<ContactDto> CreateContactAsync(CreateContactRequestDto requestDto, int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }

                if (requestDto.CategoryId != 3 && await _categoryRepository.GetByIdAsync(requestDto.CategoryId) == null)
                {
                    throw new NotFoundException("Category not found");
                }

                if (requestDto.SubcategoryId.HasValue && await _subcategoryRepository.GetByIdAsync(requestDto.SubcategoryId.Value) == null)
                {
                    throw new NotFoundException("Subcategory not found");
                }

                var contact = _mapper.Map<Contact>(requestDto);
                contact.UserId = userId;

                if (requestDto.CategoryId == 3 && !string.IsNullOrEmpty(requestDto.CustomSubcategory))
                {
                    var subcategory = new Subcategory
                    {
                        CategoryId = requestDto.CategoryId,
                        Name = requestDto.CustomSubcategory
                    };
                    await _subcategoryRepository.AddAsync(subcategory);
                    contact.SubcategoryId = subcategory.SubcategoryId;
                }
                else
                {
                    contact.CustomSubcategory = string.Empty;
                }

                contact = await _retryStrategy.ExecuteWithRetriesAsync(async () => await _contactRepository.AddAsync(contact), nameof(CreateContactAsync), _retryPolicyConfig.MaxRetries, _retryPolicyConfig.BaseDelay);
                return _mapper.Map<ContactDto>(contact);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during contact creation: {ErrorMessage}", ex.InnerException?.Message);
                throw new DataException("Database error during contact creation.", ex);
            }
        }

        // Aktualizacja istniejącego kontaktu z odpowiednimi weryfikacjami i mapowaniem
        public async Task UpdateContactAsync(int id, UpdateContactRequestDto requestDto, int userId)
        {
            try
            {
                var contact = await _contactRepository.GetByIdForUserAsync(id, userId);
                if (contact == null)
                {
                    throw new NotFoundException("Contact not found");
                }

                if (requestDto.CategoryId != 3 && await _categoryRepository.GetByIdAsync(requestDto.CategoryId) == null)
                {
                    throw new NotFoundException("Category not found");
                }

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
                _logger.LogError(ex, "Database error during contact update: {ErrorMessage}", ex.InnerException?.Message);
                throw new DataException("Database error during contact update.", ex);
            }
        }

        // Usunięcie kontaktu z zastosowaniem polityki ponawiania
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
