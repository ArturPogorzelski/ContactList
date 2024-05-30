using AutoMapper;
using ContactList.Application.Commands.Contact;
using ContactList.Application.Queries.Contact;
using ContactList.Core.Dtos;
using ContactList.Core.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactList.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactsController> _logger;
        private readonly IValidator<CreateContactRequestDto> _createContactValidator;
        private readonly IValidator<UpdateContactRequestDto> _updateContactValidator;

        public ContactsController(IMediator mediator, IMapper mapper, ILogger<ContactsController> logger, IValidator<CreateContactRequestDto> createContactValidator,
        IValidator<UpdateContactRequestDto> updateContactValidator)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _createContactValidator = createContactValidator;
            _updateContactValidator = updateContactValidator;
        }

        [HttpGet("GetAllContactsCQRS")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContacts()
        {
            try
            {
                var contacts = await _mediator.Send(new GetAllContactsQuery());
                _logger.LogInformation("Pobrano wszystkie kontakty.");
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania kontaktów.");
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        [HttpGet("ContactsForUserCQRS")]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContactsForUser()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var contacts = await _mediator.Send(new GetAllContactsForUserQuery { UserId = userId });
                _logger.LogInformation("Pobrano wszystkie kontakty użytkownika {UserId}.", userId);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania kontaktów użytkownika.");
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        [HttpGet("GetContactByIdCQRS/{id}")]
        public async Task<ActionResult<ContactDto>> GetContactById(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var contact = await _mediator.Send(new GetContactByIdQuery { ContactId = id, UserId = userId });
                _logger.LogInformation("Pobrano kontakt o ID {ContactId} dla użytkownika {UserId}", id, userId);
                return Ok(contact);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kontaktu o ID {ContactId} dla użytkownika {UserId}", id, GetUserIdFromClaims());
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania kontaktu o ID {ContactId}", id);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        [HttpPost("CreateContactCQRS")]
        public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactRequestDto createContactRequestDto)
        {
            var validationResult = await _createContactValidator.ValidateAsync(createContactRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var userId = GetUserIdFromClaims();
                var command = _mapper.Map<CreateContactCommand>(createContactRequestDto);
                command.UserId = userId; // Ustawienie userId w komendzie
                var contactDto = await _mediator.Send(command);
                _logger.LogInformation("Utworzono kontakt o ID {ContactId} dla użytkownika {UserId}", contactDto.ContactId, userId);
                return CreatedAtAction(nameof(GetContactById), new { id = contactDto.ContactId }, contactDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia kontaktu. Message: " + ex.Message);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.Message: " + ex.Message);
            }
        }

        [HttpPut("UpdateContactCQRS/{id}")]
        public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactRequestDto updateContactRequestDto)
        {
            var validationResult = await _updateContactValidator.ValidateAsync(updateContactRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var userId = GetUserIdFromClaims();
                var command = _mapper.Map<UpdateContactCommand>(updateContactRequestDto);
                command.UserId = userId; // Ustawienie userId w komendzie
                command.ContactId = id; // Ustawienie contactId w komendzie
                await _mediator.Send(command);
                _logger.LogInformation("Zaktualizowano kontakt o ID {ContactId} dla użytkownika {UserId}", id, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kontaktu o ID {ContactId} dla użytkownika {UserId}", id, GetUserIdFromClaims());
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji kontaktu o ID {ContactId}", id);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        [HttpDelete("DeleteContactCQRS/{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _mediator.Send(new DeleteContactCommand { ContactId = id});
                _logger.LogInformation("Usunięto kontakt o ID {ContactId} dla użytkownika {UserId}", id, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kontaktu o ID {ContactId} dla użytkownika {UserId}", id, GetUserIdFromClaims());
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania kontaktu o ID {ContactId}", id);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Pomocnicza metoda do pobierania userId z tokenu JWT
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new ContactList.Core.Exceptions.UnauthorizedAccessException("Nieprawidłowy token JWT.");
            }
            return userId;
        }
    }
}
