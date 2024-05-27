using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetByIdAsync(int id);
        Task<Role> GetByNameAsync(string name);
        Task<List<Role>> GetByNamesAsync(IEnumerable<string> names);
        Task<IEnumerable<Role>> GetAllAsync();
        Task<IEnumerable<Role>> FindRolesByIdsAsync(IEnumerable<int> roleIds);
    }
}
