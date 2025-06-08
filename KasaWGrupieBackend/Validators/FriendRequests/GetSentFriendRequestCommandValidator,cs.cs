using FluentValidation;
using KasaWGrupie.API.Requests.FriendRequests.Commands;

namespace KasaWGrupie.API.Validators.FriendRequests;
public class GetSentFriendRequestsCommandValidator : AbstractValidator<GetSentFriendRequestsCommand>
{
	public GetSentFriendRequestsCommandValidator()
	{
		RuleFor(x => x.UserId)
			.GreaterThan(0)
			.WithMessage("UserId must be greater than 0.");
	}
}

