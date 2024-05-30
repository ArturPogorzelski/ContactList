using ContactList.Core.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Commands.Contact
{
    public class CreateContactCommand : IRequest<ContactDto>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string CustomSubcategory { get; set; }
        public int UserId { get; set; }
        
    }
}
