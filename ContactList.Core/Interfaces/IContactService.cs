using ContactList.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<ContactDto>> GetAllContactsAsync();
        Task<IEnumerable<ContactDto>> GetAllContactsForUserAsync(int userId);
        Task<ContactDto> GetContactByIdAsync(int id, int userId);
        Task<ContactDto> CreateContactAsync(CreateContactRequestDto createContactRequestDto, int userId);
        Task UpdateContactAsync(int id, UpdateContactRequestDto updateContactRequestDto, int userId);
        Task DeleteContactAsync(int id, int userId);
    }
}
