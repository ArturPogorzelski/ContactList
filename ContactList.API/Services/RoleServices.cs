using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;

namespace ContactList.API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> GetRoleByIdAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }
            return _mapper.Map<RoleDto>(role);
        }
        public async Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
        {
            return await _roleRepository.FindRolesByIdsAsync(roleIds);
        }
        public async Task<IEnumerable<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNamesds)
        {
            return await _roleRepository.GetByNamesAsync(roleNamesds);
        }
    }
}
