using ContactList.Core.Dtos;
using ContactList.Core.Interfaces;
using FluentValidation;


namespace ContactList.Core.Validators
{



    public class UpdateContactRequestDtoValidator : AbstractValidator<UpdateContactRequestDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;

        public UpdateContactRequestDtoValidator(ICategoryRepository categoryRepository, ISubcategoryRepository subcategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;

            // Validate ContactId
            RuleFor(x => x.ContactId)
                .NotEmpty().WithMessage("Identyfikator kontaktu jest wymagany.");

            // Validate First Name
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Imię jest wymagane.")
                .MaximumLength(100).WithMessage("Imię nie może przekraczać 100 znaków.");

            // Validate Last Name
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Nazwisko jest wymagane.")
                .MaximumLength(100).WithMessage("Nazwisko nie może przekraczać 100 znaków.");

            // Validate Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Adres email jest wymagany.")
                .EmailAddress().WithMessage("Nieprawidłowy adres email.");

            // Validate Phone Number (if provided)
            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .Matches(@"^\d{9}$").WithMessage("Nieprawidłowy numer telefonu. Wprowadź 9 cyfr.");
            });

            // Validate Date of Birth (if provided)
            When(x => x.DateOfBirth.HasValue, () =>
            {
                RuleFor(x => x.DateOfBirth)
                    .LessThan(DateTime.Today).WithMessage("Data urodzenia musi być wcześniejsza niż dzisiejsza data.");
            });

            // Validate CategoryId
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Kategoria jest wymagana.")
                .MustAsync(BeAValidCategoryAsync).WithMessage("Nieprawidłowa kategoria.");

            // Validate SubcategoryId (if provided)
            When(x => x.SubcategoryId.HasValue, () =>
            {
                RuleFor(x => x.SubcategoryId)
                    .MustAsync(BeAValidSubcategoryAsync).WithMessage("Nieprawidłowa podkategoria.");
            });

            // Validate CustomSubcategory (if provided and Category is "Other")
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
                return true;

            return phoneNumber.All(char.IsDigit) && phoneNumber.Length == 9;
        }

        private async Task<bool> BeAValidCategoryAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await _categoryRepository.GetByIdAsync(categoryId) != null;
        }

        private async Task<bool> BeAValidSubcategoryAsync(int? subcategoryId, CancellationToken cancellationToken)
        {
            if (!subcategoryId.HasValue)
                return true; // Jeśli SubcategoryId jest null, to znaczy, że nie została wybrana żadna podkategoria

            return await _subcategoryRepository.GetByIdAsync(subcategoryId.Value) != null;
        }
    }
}
