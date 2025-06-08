using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

public class RequestToJoinGroupWithInviteCodeCommandValidator
	: AbstractValidator<RequestToJoinGroupWithInviteCodeCommand>
{
	public RequestToJoinGroupWithInviteCodeCommandValidator()
	{
		RuleFor(x => x.UserEmail)
			.NotEmpty().WithMessage("Email nie może być pusty ✉️✨")
			.EmailAddress().WithMessage("To nie wygląda jak email, bestie 😭💌");

		RuleFor(x => x.InviteCodeDto)
			.NotNull().WithMessage("InviteCodeDto nie może być null, błagam 🙈🌟");

		RuleFor(x => x.InviteCodeDto.Code)
			.NotEmpty().WithMessage("Kod zaproszenia musi być podany, halo! 🔐💫");
	}
}
