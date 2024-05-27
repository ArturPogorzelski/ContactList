using ContactList.Core.Dtos;
using ContactList.Core.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Validators
{
    public class CreateSubcategoryRequestDtoValidator : AbstractValidator<CreateSubcategoryRequestDto>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateSubcategoryRequestDtoValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nazwa podkategorii jest wymagana.")
                .MaximumLength(50).WithMessage("Nazwa podkategorii nie może przekraczać 50 znaków.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Identyfikator kategorii jest wymagany.")
                .MustAsync(BeAValidCategoryAsync).WithMessage("Nieprawidłowa kategoria.");
        }

        private async Task<bool> BeAValidCategoryAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await _categoryRepository.GetByIdAsync(categoryId) != null;
        }
    }
}
