using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(); // Pobranie wszystkich kategorii
        Task<Category> GetByIdAsync(int id); // Pobranie kategorii po ID
        Task<Category> GetByNameAsync(string name); // Pobranie kategorii po ID
        Task<Category> AddAsync(Category category); // Dodanie nowej kategorii
        Task UpdateAsync(Category category); // Aktualizacja istniejącej kategorii
        Task DeleteAsync(int id); // Usunięcie kategorii po ID
    }
}
