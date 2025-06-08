using FluentValidation;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Validators.Users;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
	public CreateUserDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Group name is required.")
			.MaximumLength(ValidatorConstants.CreateUserDtoConstants.NameMaxLength)
			.WithMessage($"Group name cannot exceed {ValidatorConstants.CreateUserDtoConstants.NameMaxLength} characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.MaximumLength(ValidatorConstants.CreateUserDtoConstants.EmailMaxLength)
			.WithMessage($"Email cannot exceed {ValidatorConstants.CreateUserDtoConstants.EmailMaxLength} characters.")
			.EmailAddress().WithMessage("Invalid email format.");
		
	}

}
