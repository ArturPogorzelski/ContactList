using ContactList.Core.Dtos;
using ContactList.Core.Interfaces;
using FluentValidation;

namespace ContactList.Core.Validators
{




    public class CreateContactRequestDtoValidator : AbstractValidator<CreateContactRequestDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;

        public CreateContactRequestDtoValidator(ICategoryRepository categoryRepository, ISubcategoryRepository subcategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Imię jest wymagane.")
                .MaximumLength(100).WithMessage("Imię nie może przekraczaczać 100 znaków.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Nazwisko jest wymagane.")
                .MaximumLength(100).WithMessage("Nazwisko nie może przekraczać 100 znaków.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Adres email jest wymagany.")
                .EmailAddress().WithMessage("Nieprawidłowy adres email.");

            RuleFor(x => x.PhoneNumber)
                .Must(BeAValidPhoneNumber).WithMessage("Nieprawidłowy numer telefonu. Wprowadź 9 cyfr.")
                .Unless(x => string.IsNullOrEmpty(x.PhoneNumber)); // Pomijamy walidację, jeśli numer telefonu jest pusty

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Data urodzenia musi być wcześniejsza niż dzisiejsza data.")
                .Unless(x => !x.DateOfBirth.HasValue); // Pomijamy walidację, jeśli data urodzenia nie jest podana

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Kategoria jest wymagana.")
                .MustAsync(BeAValidCategoryAsync).WithMessage("Nieprawidłowa kategoria.");

            When(x => x.SubcategoryId.HasValue, () =>
            {
              
                RuleFor(x => x.SubcategoryId)
                .MustAsync(async (subcategoryId, cancellationToken) =>
                    await BeAValidSubcategoryAsync(subcategoryId, cancellationToken))
                .WithMessage("Nieprawid\u0142owa podkategoria.");
            });

            When(x => x.CategoryId == 3 && !string.IsNullOrEmpty(x.CustomSubcategory), () =>
            {
                RuleFor(x => x.CustomSubcategory)
                    .NotEmpty().WithMessage("Podkategoria jest wymagana dla kategorii 'Inne'.")
                    .MaximumLength(100).WithMessage("Podkategoria nie może przekraczać 100 znaków.");
            });
        }

        private bool BeAValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return true; // Pomijamy walidację, jeśli numer telefonu jest pusty

            return phoneNumber.All(char.IsDigit) && phoneNumber.Length == 9; // Sprawdzamy, czy składa się tylko z cyfr i ma 9 znaków
        }

        private async Task<bool> BeAValidCategoryAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await _categoryRepository.GetByIdAsync(categoryId) != null;
        }

        private async Task<bool> BeAValidSubcategoryAsync(int? subcategoryId, CancellationToken cancellationToken)
        {
            // Jeśli SubcategoryId jest null, to znaczy, że nie została wybrana żadna podkategoria
            // W takim przypadku, walidacja przechodzi pomyślnie
            if (!subcategoryId.HasValue)
                return true;

            return await _subcategoryRepository.GetByIdAsync(subcategoryId.Value) != null;
        }
    }
}
