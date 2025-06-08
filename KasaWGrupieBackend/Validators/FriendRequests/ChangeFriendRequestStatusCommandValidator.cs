using FluentValidation;
using KasaWGrupie.API.Requests.FriendRequests.Commands;

public class ChangeFriendRequestStatusCommandValidator : AbstractValidator<ChangeFriendRequestStatusCommand>
{
	public ChangeFriendRequestStatusCommandValidator()
	{
		RuleFor(x => x.RequestId)
			.GreaterThan(0).WithMessage("RequestId must be greater than 0.");

		RuleFor(x => x.ChangeFriendRequestStatusDto)
			.SetValidator(new ChangeFriendRequestStatusDtoValidator());
	}
}