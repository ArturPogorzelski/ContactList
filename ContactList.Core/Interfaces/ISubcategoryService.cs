using ContactList.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface ISubcategoryService
    {
        Task<IEnumerable<SubcategoryDto>> GetSubcategoriesByCategoryIdAsync(int categoryId);
        Task<SubcategoryDto> GetSubcategoryByIdAsync(int subcategoryId);
        Task<SubcategoryDto> CreateSubcategoryAsync(CreateSubcategoryRequestDto createSubcategoryRequestDto);
        Task UpdateSubcategoryAsync(int subcategoryId, UpdateSubcategoryRequestDto updateSubcategoryRequestDto);
        Task DeleteSubcategoryAsync(int subcategoryId);
    }
    
}
