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
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")] // Tylko administrator może zarządzać podkategoriami
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IValidator<CreateCategoryRequestDto> _createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequestDto> _updateCategoryValidator;

        public CategoriesController(
            ICategoryService categoryService,
            IMapper mapper,
            ILogger<CategoriesController> logger,
            IValidator<CreateCategoryRequestDto> createCategoryValidator,
            IValidator<UpdateCategoryRequestDto> updateCategoryValidator)
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _logger = logger;
            _createCategoryValidator = createCategoryValidator;
            _updateCategoryValidator = updateCategoryValidator;
        }

        // Pobranie wszystkich kategorii (z podkategoriami)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania kategorii.");
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Pobranie kategorii po ID (z podkategoriami)
        [HttpGet("{categoryId:int}")] // Dodajemy ograniczenie typu int dla categoryId
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kategorii o ID {CategoryId}", categoryId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania kategorii o ID {CategoryId}", categoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Utworzenie nowej kategorii
        [HttpPost]
        [Authorize(Roles = "Admin")] // Tylko dla administratorów
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequestDto createCategoryRequestDto)
        {
            var validationResult = await _createCategoryValidator.ValidateAsync(createCategoryRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var categoryDto = await _categoryService.CreateCategoryAsync(createCategoryRequestDto);
                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = categoryDto.CategoryId }, categoryDto);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Nieprawidłowe dane podczas tworzenia kategorii: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia kategorii.");
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Aktualizacja kategorii
        [HttpPut("{categoryId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] UpdateCategoryRequestDto updateCategoryRequestDto)
        {
            if (categoryId != updateCategoryRequestDto.CategoryId)
                return BadRequest("Niezgodność identyfikatorów kategorii.");

            var validationResult = await _updateCategoryValidator.ValidateAsync(updateCategoryRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                await _categoryService.UpdateCategoryAsync(categoryId, updateCategoryRequestDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kategorii o ID {CategoryId}", categoryId);
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Nieprawidłowe dane podczas aktualizacji kategorii: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji kategorii o ID {CategoryId}", categoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }

        // Usunięcie kategorii
        [HttpDelete("{categoryId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(categoryId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono kategorii o ID {CategoryId}", categoryId);
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex) // Dodana obsługa BadRequestException
            {
                _logger.LogWarning(ex, "Nie można usunąć kategorii o ID {CategoryId}: {ErrorMessage}", categoryId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania kategorii o ID {CategoryId}", categoryId);
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera.");
            }
        }
    }
}
