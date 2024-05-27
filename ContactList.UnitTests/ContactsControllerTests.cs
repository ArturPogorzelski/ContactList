using AutoMapper;
using ContactList.API.Controllers.v1;
using ContactList.API.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.UnitTests
{

    public class ContactsControllerTests
    {
        private readonly Mock<IContactService> _contactServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ContactsController>> _loggerMock;
        private readonly Mock<IValidator<CreateContactRequestDto>> _createContactValidatorMock;
        private readonly Mock<IValidator<UpdateContactRequestDto>> _updateContactValidatorMock;
        private readonly ContactsController _controller;

        public ContactsControllerTests()
        {
            _contactServiceMock = new Mock<IContactService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ContactsController>>();
            _createContactValidatorMock = new Mock<IValidator<CreateContactRequestDto>>();
            _updateContactValidatorMock = new Mock<IValidator<UpdateContactRequestDto>>();

            _controller = new ContactsController(
                _contactServiceMock.Object,
                _categoryServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _createContactValidatorMock.Object,
                _updateContactValidatorMock.Object);
        }

        [Fact]
        public async Task GetAllContacts_ReturnsOkWithContactDtos()
        {
            // Arrange
            var contacts = new List<ContactDto> { new ContactDto(), new ContactDto() };
            _contactServiceMock.Setup(cs => cs.GetAllContactsAsync()).ReturnsAsync(contacts);

            // Act
            var result = await _controller.GetAllContacts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContacts = Assert.IsAssignableFrom<IEnumerable<ContactDto>>(okResult.Value);
            Assert.Equal(contacts, returnedContacts);
        }

        [Fact]
        public async Task GetAllContactsForUser_ReturnsOkWithContactDtos()
        {
            // Arrange
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            var contacts = new List<ContactDto> { new ContactDto(), new ContactDto() };
            _contactServiceMock.Setup(cs => cs.GetAllContactsForUserAsync(userId)).ReturnsAsync(contacts);

            // Act
            var result = await _controller.GetAllContactsForUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContacts = Assert.IsAssignableFrom<IEnumerable<ContactDto>>(okResult.Value);
            Assert.Equal(contacts, returnedContacts);
        }

        [Fact]
        public async Task GetContactById_ExistingContact_ReturnsOkWithContactDto()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            var contactDto = new ContactDto { ContactId = contactId };
            _contactServiceMock.Setup(cs => cs.GetContactByIdAsync(contactId, userId)).ReturnsAsync(contactDto);

            // Act
            var result = await _controller.GetContactById(contactId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContact = Assert.IsType<ContactDto>(okResult.Value);
            Assert.Equal(contactDto, returnedContact);
        }

        [Fact]
        public async Task GetContactById_NonExistingContact_ReturnsNotFound()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            _contactServiceMock.Setup(cs => cs.GetContactByIdAsync(contactId, userId)).ThrowsAsync(new NotFoundException("Contact not found."));

            // Act
            var result = await _controller.GetContactById(contactId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateContact_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            var createContactRequestDto = new CreateContactRequestDto { /* ... dane kontaktu ... */ };
            var contactDto = new ContactDto { ContactId = 1 };
            _createContactValidatorMock.Setup(v => v.ValidateAsync(createContactRequestDto, CancellationToken.None)).ReturnsAsync(new ValidationResult());
            _contactServiceMock.Setup(cs => cs.CreateContactAsync(createContactRequestDto, userId)).ReturnsAsync(contactDto);

            // Act
            var result = await _controller.CreateContact(createContactRequestDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ContactsController.GetContactById), createdAtActionResult.ActionName);
            Assert.Equal(contactDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateContact_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var createContactRequestDto = new CreateContactRequestDto(); // Nieprawidłowe dane
            var validationResult = new ValidationResult(new[] { new ValidationFailure("FirstName", "Imię jest wymagane.") });
            _createContactValidatorMock.Setup(v => v.ValidateAsync(createContactRequestDto, CancellationToken.None)).ReturnsAsync(validationResult);

            // Act
            var result = await _controller.CreateContact(createContactRequestDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(validationResult.Errors, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateContact_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            var updateContactRequestDto = new UpdateContactRequestDto { ContactId = contactId, /* ... dane kontaktu ... */ };
            _updateContactValidatorMock.Setup(v => v.ValidateAsync(updateContactRequestDto, CancellationToken.None)).ReturnsAsync(new ValidationResult());
            _contactServiceMock.Setup(cs => cs.UpdateContactAsync(contactId, updateContactRequestDto, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateContact(contactId, updateContactRequestDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async Task UpdateContact_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var updateContactRequestDto = new UpdateContactRequestDto(); // Nieprawidłowe dane
            var validationResult = new ValidationResult(new[] { new ValidationFailure("FirstName", "Imię jest wymagane.") });
            _updateContactValidatorMock.Setup(v => v.ValidateAsync(updateContactRequestDto, CancellationToken.None)).ReturnsAsync(validationResult);

            // Act
            var result = await _controller.UpdateContact(1, updateContactRequestDto); // Dowolne ID kontaktu

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(validationResult.Errors, badRequestResult.Value); // Sprawdzamy czy zwrócone błędy są takie same jak błędy walidacji
        }
        [Fact]
        public async Task DeleteContact_ExistingContact_ReturnsNoContent()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            _contactServiceMock.Setup(cs => cs.DeleteContactAsync(contactId, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteContact(contactId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteContact_NonExistingContact_ReturnsNotFound()
        {
            // Arrange
            var contactId = 1;
            var userId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }, "Bearer"))
                }
            };
            _contactServiceMock.Setup(cs => cs.DeleteContactAsync(contactId, userId)).ThrowsAsync(new NotFoundException("Contact not found."));

            // Act
            var result = await _controller.DeleteContact(contactId);

            // Assert
            //Assert.IsType<NotFoundObjectResult>(result.Result); // result.Result ponieważ DeleteContact zwraca ActionResult<T>, a nie bezpośrednio T
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
