using ContactList.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int categoryId);
        Task<CategoryDto> GetCategoryByNameAsync(string name);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto createCategoryRequestDto);
        Task UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto updateCategoryRequestDto);
        Task DeleteCategoryAsync(int categoryId);
    }
}
