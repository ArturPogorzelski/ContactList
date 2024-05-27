using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Dtos
{
    public class SubcategoryDto
    {
        public int SubcategoryId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }
    }
}
