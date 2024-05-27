using AutoMapper;
using ContactList.API.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;

namespace ContactList.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]

public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ICategoryService _categoryService; // Dodajemy serwis kategorii
    private readonly IMapper _mapper;
    private readonly ILogger<ContactsController> _logger;
    private readonly IValidator<CreateContactRequestDto> _createContactValidator;
    private readonly IValidator<UpdateContactRequestDto> _updateContactValidator;

    public ContactsController(
        IContactService contactService,
        ICategoryService categoryService, // Wstrzykujemy serwis kategorii
        IMapper mapper,
        ILogger<ContactsController> logger,
        IValidator<CreateContactRequestDto> createContactValidator,
        IValidator<UpdateContactRequestDto> updateContactValidator)
    {
        _contactService = contactService;
        _categoryService = categoryService;
        _mapper = mapper;
        _logger = logger;
        _createContactValidator = createContactValidator;
        _updateContactValidator = updateContactValidator;
    }

    // Pobranie wszystkich kontaktów użytkownika
    [HttpGet("GetAllContacts")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContacts()
    {
        try
        {
           
            var contacts = await _contactService.GetAllContactsAsync();
            _logger.LogInformation("Pobrano wszystkie kontakty.");
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania kontaktów.");
            return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
        }
    }

    // Pobranie wszystkich kontaktów użytkownika
    [HttpGet("ContactsForUser")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContactsForUser()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var contacts = await _contactService.GetAllContactsForUserAsync(userId);
            _logger.LogInformation("Pobrano wszystkie kontakty użytkownika {UserId}.", userId);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania kontaktów użytkownika.");
            return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
        }
    }

    // Pobranie kontaktu po ID
    [HttpGet("GetContactById/{id}")]
    [Authorize]
    public async Task<ActionResult<ContactDto>> GetContactById(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var contact = await _contactService.GetContactByIdAsync(id, userId);
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

    // Utworzenie nowego kontaktu (z obsługą kategorii i podkategorii)
    [HttpPost("CreateContact")]
    [Authorize]
    public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactRequestDto createContactRequestDto)
    {
        var validationResult = await _createContactValidator.ValidateAsync(createContactRequestDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors); // Zwraca BadRequest z błędami walidacji

        try
        {
            var userId = GetUserIdFromClaims();
            var contactDto = await _contactService.CreateContactAsync(createContactRequestDto, userId);
            _logger.LogInformation("Utworzono kontakt o ID {ContactId} dla użytkownika {UserId}", contactDto.ContactId, userId);
            return CreatedAtAction(nameof(GetContactById), new { id = contactDto.ContactId }, contactDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia kontaktu. Message: "+ ex.Message);
            return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.Message: " + ex.Message);
        }
    }

    // Aktualizacja kontaktu (z obsługą kategorii i podkategorii)
    [HttpPut("UpdateContact/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactRequestDto updateContactRequestDto)
    {
        var validationResult = await _updateContactValidator.ValidateAsync(updateContactRequestDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors); // Zwraca BadRequest z błędami walidacji

        try
        {
            var userId = GetUserIdFromClaims();
            await _contactService.UpdateContactAsync(id, updateContactRequestDto, userId);
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

    // Usunięcie kontaktu
    [HttpDelete("DeleteContact/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteContact(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            await _contactService.DeleteContactAsync(id, userId);
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
    [HttpGet("test")]
    public IActionResult Test()
    {
        var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
        _logger.LogInformation("Claims: " + string.Join(", ", claims));

        var userId = GetUserIdFromClaims();
        return Ok(new { UserId = userId });
    }

    // Pomocnicza metoda do pobierania userId z tokenu JWT
    private int GetUserIdFromClaims()
    {
        var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
        _logger.LogInformation("Claims: " + string.Join(", ", claims));

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new ContactList.Core.Exceptions.UnauthorizedAccessException("Nieprawidłowy token JWT.");
        }
        return userId;
    }
}
