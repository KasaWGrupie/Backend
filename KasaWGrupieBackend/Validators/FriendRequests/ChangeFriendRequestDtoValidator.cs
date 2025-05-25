using FluentValidation;
using KasaWGrupie.API.DTOs.FriendRequest;
using KasaWGrupie.Core.Enums;

public class ChangeFriendRequestStatusDtoValidator : AbstractValidator<ChangeFriendRequestStatusDto>
{
	public ChangeFriendRequestStatusDtoValidator()
	{
		RuleFor(x => x.Status)
			.NotEmpty().WithMessage("Status is required.")
			.IsEnumName(typeof(FriendRequestStatus), caseSensitive: false)
			.WithMessage("Invalid status value.");
	}
}