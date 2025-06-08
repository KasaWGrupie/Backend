using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

public class GetGroupInviteCodeCommandValidator : AbstractValidator<GetGroupInviteCodeCommand>
{
	public GetGroupInviteCodeCommandValidator()
	{
		RuleFor(x => x.GroupId)
			.GreaterThan(0)
			.WithMessage("GroupId must be greater than 0 ⭐");
	}
}