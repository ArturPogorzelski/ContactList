using ContactList.Core.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Validators
{
    public class CreateCategoryRequestDtoValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCategoryRequestDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        }
    }
}
