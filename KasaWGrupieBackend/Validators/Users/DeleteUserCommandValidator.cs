using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
	public DeleteUserCommandValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0)
			.WithMessage("User Id must be greater than 0.");
	}
}