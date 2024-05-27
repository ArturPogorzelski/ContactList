using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    // Defines the contract for data access operations on Contact entities.
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllAsync();
        Task<IEnumerable<Contact>> GetAllForUserAsync(int userId);
            Task<Contact> GetByIdForUserAsync(int id, int userId);
            Task<Contact> AddAsync(Contact contact);
            Task UpdateAsync(Contact contact);
            Task DeleteAsync(int id);
       
    }
}
