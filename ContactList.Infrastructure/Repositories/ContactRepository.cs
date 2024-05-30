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
    public class ContactRepository : IContactRepository
    {
        private readonly ContactListDbContext _context;

        public ContactRepository(ContactListDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            return await _context.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contact>> GetAllForUserAsync(int userId)
        {
            return await _context.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Contact> GetByIdForUserAsync(int id, int userId)
        {
            return await _context.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .FirstOrDefaultAsync(c => c.ContactId == id && c.UserId == userId);
        }

        
             public async Task<Contact> GetByIdAsync(int id)
        {
            return await _context.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .FirstOrDefaultAsync(c => c.ContactId == id);
        }
        public async Task<Contact> AddAsync(Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task UpdateAsync(Contact contact)
        {
            _context.Entry(contact).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }
    }
}
