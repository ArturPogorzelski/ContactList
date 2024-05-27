using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class UpdateSubcategoryRequestDto : CreateSubcategoryRequestDto
    {
        [Required(ErrorMessage = "Identyfikator podkategorii jest wymagany.")]
        public int SubcategoryId { get; set; }
    }
}
