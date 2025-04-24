using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

public sealed class GetUserByEmailCommandValidator : AbstractValidator<GetUserByEmailCommand>
{
	public GetUserByEmailCommandValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.");
	}
}