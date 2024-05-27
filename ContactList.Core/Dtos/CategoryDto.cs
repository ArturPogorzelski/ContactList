using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public List<SubcategoryDto>? Subcategories { get; set; } // Lista podkategorii (opcjonalna)
    }
}
