using ContactList.Core.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Validators
{
    public class UpdateSubcategoryRequestDtoValidator : AbstractValidator<UpdateSubcategoryRequestDto>
    {
        public UpdateSubcategoryRequestDtoValidator()
        {
            Include(new CreateSubcategoryRequestDtoValidator(null!)); // Reusing the create validator
            RuleFor(x => x.SubcategoryId).NotEmpty().WithMessage("Identyfikator podkategorii jest wymagany.");
        }
    }
}
