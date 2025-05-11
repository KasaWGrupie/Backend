using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class GetFriendsCommandValidator : AbstractValidator<GetFriendsCommand>
{
	public GetFriendsCommandValidator()
	{
		RuleFor(x => x.UserId)
			.GreaterThan(0)
			.WithMessage("UserId must be greater than 0.");
	}
}
