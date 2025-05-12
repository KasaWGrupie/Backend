using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class SearchUsersByPartialEmailCommandValidator : AbstractValidator<SearchUsersByPartialEmailCommand>
{
    public SearchUsersByPartialEmailCommandValidator()
    {
        // bez sprawdzania formatu email, bo można podać fragment
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(ValidatorConstants.CreateUserDtoConstants.EmailMaxLength)
            .WithMessage($"Email cannot exceed {ValidatorConstants.CreateUserDtoConstants.EmailMaxLength} characters.");
    }
}