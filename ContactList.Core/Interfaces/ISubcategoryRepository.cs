using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface ISubcategoryRepository
    {
        Task<IEnumerable<Subcategory>> GetByCategoryIdAsync(int categoryId);
        Task<Subcategory> GetByIdAsync(int id);
        Task<Subcategory> GetByNameAsync(string name);
        Task<Subcategory> AddAsync(Subcategory subcategory);
        Task UpdateAsync(Subcategory subcategory);
        Task DeleteAsync(int id);
    }
}
