using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class GetUserGroupsCommandValidator : AbstractValidator<GetUserGroupsCommand>
{
    public GetUserGroupsCommandValidator()
    {
        RuleFor(x => x.UserEmail)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(ValidatorConstants.CreateUserDtoConstants.EmailMaxLength)
            .WithMessage($"Email cannot exceed {ValidatorConstants.CreateUserDtoConstants.EmailMaxLength} characters.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
