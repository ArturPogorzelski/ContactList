using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ContactListDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(ContactListDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                   .ToListAsync();
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while saving changes: {InnerException}", ex.InnerException?.Message);
                throw; // Rzucanie wyjątku dalej lub obsługa błędu
            }
            
            return user;
        }

        public async Task<IList<string>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return userRoles ?? new List<string>(); // Zwróć pustą listę, jeśli nie ma ról
        }



    }
}
