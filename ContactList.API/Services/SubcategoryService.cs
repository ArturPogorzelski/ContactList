using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;

namespace ContactList.API.Services
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public SubcategoryService(ISubcategoryRepository subcategoryRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _subcategoryRepository = subcategoryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubcategoryDto>> GetSubcategoriesByCategoryIdAsync(int categoryId)
        {
            var subcategories = await _subcategoryRepository.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<SubcategoryDto>>(subcategories);
        }

        public async Task<SubcategoryDto> GetSubcategoryByIdAsync(int subcategoryId)
        {
            var subcategory = await _subcategoryRepository.GetByIdAsync(subcategoryId);
            if (subcategory == null)
            {
                throw new NotFoundException("Subcategory not found.");
            }
            return _mapper.Map<SubcategoryDto>(subcategory);
        }

        public async Task<SubcategoryDto> CreateSubcategoryAsync(CreateSubcategoryRequestDto createSubcategoryRequestDto)
        {
            // Sprawdzenie, czy kategoria istnieje
            if (await _categoryRepository.GetByIdAsync(createSubcategoryRequestDto.CategoryId) == null)
            {
                throw new NotFoundException("Category not found.");
            }

            var subcategory = _mapper.Map<Subcategory>(createSubcategoryRequestDto);
            subcategory = await _subcategoryRepository.AddAsync(subcategory);
            return _mapper.Map<SubcategoryDto>(subcategory);
        }

        public async Task UpdateSubcategoryAsync(int subcategoryId, UpdateSubcategoryRequestDto updateSubcategoryRequestDto)
        {
            var subcategory = await _subcategoryRepository.GetByIdAsync(subcategoryId);
            if (subcategory == null)
            {
                throw new NotFoundException("Subcategory not found.");
            }

            // Sprawdzenie, czy kategoria istnieje
            if (await _categoryRepository.GetByIdAsync(updateSubcategoryRequestDto.CategoryId) == null)
            {
                throw new NotFoundException("Category not found.");
            }

            _mapper.Map(updateSubcategoryRequestDto, subcategory);
            await _subcategoryRepository.UpdateAsync(subcategory);
        }

        public async Task DeleteSubcategoryAsync(int subcategoryId)
        {
            var subcategory = await _subcategoryRepository.GetByIdAsync(subcategoryId);
            if (subcategory == null)
            {
                throw new NotFoundException("Subcategory not found.");
            }

            await _subcategoryRepository.DeleteAsync(subcategoryId);
        }
    }
}
