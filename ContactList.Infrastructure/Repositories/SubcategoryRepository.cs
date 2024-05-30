using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Repositories
{
    public class SubcategoryRepository : ISubcategoryRepository
    {
        private readonly ContactListDbContext _context;

        public SubcategoryRepository(ContactListDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subcategory>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Subcategories
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Subcategory> GetByIdAsync(int id)
        {
            return await _context.Subcategories.FindAsync(id);
        }
        public async Task<Subcategory> GetByNameAsync(string name)
        {
            return await _context.Subcategories.Where(x=>x.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Subcategory> AddAsync(Subcategory subcategory)
        {
            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();
            return subcategory;
        }

        public async Task UpdateAsync(Subcategory subcategory)
        {
            _context.Entry(subcategory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory != null)
            {
                _context.Subcategories.Remove(subcategory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
