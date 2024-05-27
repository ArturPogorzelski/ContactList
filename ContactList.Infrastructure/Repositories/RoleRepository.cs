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
    public class RoleRepository : IRoleRepository
    {
        private readonly ContactListDbContext _context;

        public RoleRepository(ContactListDbContext context)
        {
            _context = context;
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            return await _context.Roles.SingleOrDefaultAsync(r => r.Name == name);
        }
        public async Task<Role> GetByIdAsync(int id)
        {
            return await _context.Roles.SingleOrDefaultAsync(r => r.RoleId == id);
        }
        public async Task<List<Role>> GetByNamesAsync(IEnumerable<string> names)
        {
            return await _context.Roles.Where(r => names.Contains(r.Name)).ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .ToListAsync();
        }
        public async Task<IEnumerable<Role>> FindRolesByIdsAsync(IEnumerable<int> roleIds)
        {
            return await _context.Set<Role>().Where(role => roleIds.Contains(role.RoleId)).ToListAsync();
        }
    }
}
