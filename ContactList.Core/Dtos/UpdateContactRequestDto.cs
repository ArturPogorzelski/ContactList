using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class UpdateContactRequestDto : CreateContactRequestDto
    {
        [Required(ErrorMessage = "Identyfikator kontaktu jest wymagany.")]
        public int ContactId { get; set; }
    }
}
