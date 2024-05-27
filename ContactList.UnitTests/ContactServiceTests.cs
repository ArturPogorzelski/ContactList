using AutoMapper;
using ContactList.API.Configuration;
using ContactList.API.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using ContactList.Infrastructure.Helpers.Strategy;
using ContactList.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.UnitTests
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _contactRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ISubcategoryRepository> _subcategoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRetryStrategy> _retryStrategyMock;
        private readonly Mock<IRetryHelper> _retryHelperMock;
        private readonly Mock<ILogger<ContactService>> _loggerMock;
        private readonly ContactService _contactService;

        public ContactServiceTests()
        {
            _contactRepositoryMock = new Mock<IContactRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _subcategoryRepositoryMock = new Mock<ISubcategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _retryStrategyMock = new Mock<IRetryStrategy>();
            _retryHelperMock = new Mock<IRetryHelper>();
            _loggerMock = new Mock<ILogger<ContactService>>();

            // Konfiguracja mocka dla IOptions<RetryPolicyConfig>
            var retryPolicyConfig = new RetryPolicyConfig { MaxRetries = 3, BaseDelay = 1000 };
            var retryPolicyConfigOptions = Options.Create(retryPolicyConfig);

            _contactService = new ContactService(
                _contactRepositoryMock.Object,
                _mapperMock.Object,
                _userRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _subcategoryRepositoryMock.Object,
                _loggerMock.Object,
                retryPolicyConfigOptions,
                _retryHelperMock.Object,
                _retryStrategyMock.Object
            );
        }


        // CreateContactAsync
        [Fact]
        public async Task CreateContactAsync_ValidRequest_CreatesContactAndReturnsDto()
        {
            // Arrange
            var userId = 1;
            var user = new User { UserId = userId };
            var requestDto = new CreateContactRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123456789",
                CategoryId = 1, // Business category
                SubcategoryId = 1 // Boss subcategory
            };
            var contact = new Contact { UserId = userId };
            var contactDto = new ContactDto { ContactId = 1 };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(requestDto.CategoryId)).ReturnsAsync(new Category { CategoryId = 1, Name = "Business" });
            _subcategoryRepositoryMock.Setup(repo => repo.GetByIdAsync(requestDto.SubcategoryId.Value)).ReturnsAsync(new Subcategory { SubcategoryId = 1, CategoryId = 1, Name = "Boss" });
            _mapperMock.Setup(mapper => mapper.Map<Contact>(requestDto)).Returns(contact);
            _contactRepositoryMock.Setup(repo => repo.AddAsync(contact)).ReturnsAsync(contact);
            _mapperMock.Setup(mapper => mapper.Map<ContactDto>(contact)).Returns(contactDto);
            _retryStrategyMock
                .Setup(strategy => strategy.ExecuteWithRetriesAsync(It.IsAny<Func<Task<Contact>>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(contact);

            // Act
            var result = await _contactService.CreateContactAsync(requestDto, userId);

            // Assert
            Assert.Equal(contactDto, result);
        }

        [Fact]
        public async Task CreateContactAsync_InvalidUser_ThrowsNotFoundException()
        {
            // Arrange
            var userId = 1;
            var requestDto = new CreateContactRequestDto();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _contactService.CreateContactAsync(requestDto, userId));
        }
       
      

        [Fact]
        public async Task DeleteContactAsync_NonExistingContact_ThrowsNotFoundException()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _contactRepositoryMock.Setup(repo => repo.GetByIdForUserAsync(contactId, userId)).ReturnsAsync((Contact)null); // Kontakt nie istnieje

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _contactService.DeleteContactAsync(contactId, userId)); // Oczekujemy wyjątku NotFoundException
        }
    }
}
