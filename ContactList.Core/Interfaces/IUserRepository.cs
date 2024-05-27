using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int id);
        Task<User> AddAsync(User user);
        Task<IList<string>> GetUserRolesAsync(int userId);
        Task<IEnumerable<User>> GetAllAsync();
    }
}
