using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactList.API.Controllers
{
    [ApiController]
    [Route("api/v1/categories/{categoryId:int}/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin")] // Tylko administrator może zarządzać podkategoriami
    public class SubcategoriesController : ControllerBase
    {
        private readonly ISubcategoryService _subcategoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<SubcategoriesController> _logger;
        private readonly IValidator<CreateSubcategoryRequestDto> _createSubcategoryValidator;
        private readonly IValidator<UpdateSubcategoryRequestDto> _updateSubcategoryValidator;

        public SubcategoriesController(
            ISubcategoryService subcategoryService,
            IMapper mapper,
            ILogger<SubcategoriesController> logger,
            IValidator<CreateSubcategoryRequestDto> createSubcategoryValidator,
            IValidator<UpdateSubcategoryRequestDto> updateSubcategoryValidator)
        {
            _subcategoryService = subcategoryService;
            _mapper = mapper;
            _logger = logger;
            _createSubcategoryValidator = createSubcategoryValidator;
            _updateSubcategoryValidator = updateSubcategoryValidator;
        }


        // Pobranie wszystkich podkategorii dla danej kategorii
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubcategoryDto>>> GetAllSubcategories(int categoryId)
        {
            try
            {
                var subcategories = await _subcategoryService.GetSubcategoriesByCategoryIdAsync(categoryId);
                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania podkategorii dla kategorii o ID {CategoryId}.", categoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Pobranie podkategorii po ID
        [HttpGet("{subcategoryId:int}")]
        public async Task<ActionResult<SubcategoryDto>> GetSubcategoryById(int categoryId, int subcategoryId)
        {
            try
            {
                var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(subcategoryId);
                if (subcategory.CategoryId != categoryId)
                {
                    return BadRequest("Podkategoria nie należy do podanej kategorii.");
                }
                return Ok(subcategory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono podkategorii o ID {SubcategoryId} dla kategorii o ID {CategoryId}.", subcategoryId, categoryId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania podkategorii o ID {SubcategoryId}.", subcategoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Utworzenie nowej podkategorii
        [HttpPost]
        public async Task<ActionResult<SubcategoryDto>> CreateSubcategory(int categoryId, [FromBody] CreateSubcategoryRequestDto createSubcategoryRequestDto)
        {
            var validationResult = await _createSubcategoryValidator.ValidateAsync(createSubcategoryRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                createSubcategoryRequestDto.CategoryId = categoryId; // Ustawiamy CategoryId na podstawie trasy
                var subcategoryDto = await _subcategoryService.CreateSubcategoryAsync(createSubcategoryRequestDto);
                return CreatedAtAction(nameof(GetSubcategoryById), new { categoryId = categoryId, subcategoryId = subcategoryDto.SubcategoryId }, subcategoryDto);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kategorii o ID {CategoryId} podczas tworzenia podkategorii.", categoryId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia podkategorii.");
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Aktualizacja podkategorii
        [HttpPut("{subcategoryId:int}")]
        public async Task<IActionResult> UpdateSubcategory(int categoryId, int subcategoryId, [FromBody] UpdateSubcategoryRequestDto updateSubcategoryRequestDto)
        {
            if (categoryId != updateSubcategoryRequestDto.CategoryId || subcategoryId != updateSubcategoryRequestDto.SubcategoryId)
                return BadRequest("Niezgodność identyfikatorów.");

            var validationResult = await _updateSubcategoryValidator.ValidateAsync(updateSubcategoryRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                await _subcategoryService.UpdateSubcategoryAsync(subcategoryId, updateSubcategoryRequestDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono podkategorii o ID {SubcategoryId}", subcategoryId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji podkategorii o ID {SubcategoryId}.", subcategoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Usunięcie podkategorii
        [HttpDelete("{subcategoryId:int}")]
        public async Task<IActionResult> DeleteSubcategory(int categoryId, int subcategoryId)
        {
            try
            {
                var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(subcategoryId);
                if (subcategory.CategoryId != categoryId)
                {
                    return BadRequest("Podkategoria nie należy do podanej kategorii.");
                }
                await _subcategoryService.DeleteSubcategoryAsync(subcategoryId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono podkategorii o ID {SubcategoryId} dla kategorii o ID {CategoryId}.", subcategoryId, categoryId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania podkategorii o ID {SubcategoryId}.", subcategoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }
    }
}
