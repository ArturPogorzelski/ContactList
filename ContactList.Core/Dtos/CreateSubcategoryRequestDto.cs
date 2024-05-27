using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class CreateSubcategoryRequestDto
    {
        [Required(ErrorMessage = "Nazwa podkategorii jest wymagana.")]
        [MaxLength(50, ErrorMessage = "Nazwa podkategorii nie może przekraczać 50 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Identyfikator kategorii jest wymagany.")]
        public int CategoryId { get; set; }
    }
}
