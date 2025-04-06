using FluentValidation;
using KasaWGrupie.API.DTOs.FriendRequest;

public class AddFriendRequestDtoValidator : AbstractValidator<AddFriendRequestDto>
{
	public AddFriendRequestDtoValidator()
	{
		RuleFor(x => x.SenderId)
			.GreaterThan(0)
			.WithMessage("SenderId must be greater than 0.");

		RuleFor(x => x.ReceiverId)
			.GreaterThan(0)
			.WithMessage("ReceiverId must be greater than 0.");

		RuleFor(x => x)
			.Must(x => x.SenderId != x.ReceiverId)
			.WithMessage("Sender and Receiver cannot be the same user.");
	}
}