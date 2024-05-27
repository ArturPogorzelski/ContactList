using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int roleId);
        Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<int> roleIds);
        Task<IEnumerable<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames);

    }
}
