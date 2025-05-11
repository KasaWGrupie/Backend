using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public sealed class GetUserByIdCommandValidator : AbstractValidator<GetUserByIdCommand>
{
	public GetUserByIdCommandValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0)
			.WithMessage("User Id must be greater than 0.");
	}
}