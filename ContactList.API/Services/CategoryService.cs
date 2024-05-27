using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContactList.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ContactListDbContext _context;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ContactListDbContext context)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }
            return _mapper.Map<CategoryDto>(category);
        }
        public async Task<CategoryDto> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }
            return _mapper.Map<CategoryDto>(category);
        }
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto createCategoryRequestDto)
        {
            // Walidacja unikalności nazwy kategorii
            if (await _categoryRepository.GetByNameAsync(createCategoryRequestDto.Name) != null)
            {
                throw new BadRequestException("Kategoria o podanej nazwie już istnieje.");
            }

            var category = _mapper.Map<Category>(createCategoryRequestDto);
            category = await _categoryRepository.AddAsync(category);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto updateCategoryRequestDto)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }

            // Walidacja unikalności nazwy kategorii (oprócz samej siebie)
            var existingCategory = await _categoryRepository.GetByNameAsync(updateCategoryRequestDto.Name);
            if (existingCategory != null && existingCategory.CategoryId != categoryId)
            {
                throw new BadRequestException("Kategoria o podanej nazwie już istnieje.");
            }

            _mapper.Map(updateCategoryRequestDto, category);
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }

            // Sprawdzenie, czy kategoria nie jest używana przez żadne kontakty
            if (await _context.Contacts.AnyAsync(c => c.CategoryId == categoryId))
            {
                throw new BadRequestException("Nie można usunąć kategorii, ponieważ jest ona używana przez kontakty.");
            }

            await _categoryRepository.DeleteAsync(categoryId);
        }
    }
}
