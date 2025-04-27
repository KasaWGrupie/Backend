using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class UpdateUserNameCommandValidator : AbstractValidator<UpdateUserNameCommand>
{
	public UpdateUserNameCommandValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0)
			.WithMessage("User Id must be greater than 0.");

		RuleFor(x => x.UpdateUserNameDto)
			.NotNull()
			.WithMessage("UpdateUserNameDto cannot be null.");

		RuleFor(x => x.UpdateUserNameDto.Name)
			.NotEmpty()
			.WithMessage("Name cannot be empty.")
			.MaximumLength(100)
			.WithMessage("Name cannot exceed 100 characters.");
	}
}