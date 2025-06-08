using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups;

public class RemoveUserFromGroupCommandValidator
  : AbstractValidator<RemoveUserFromGroupCommand>
{
    public RemoveUserFromGroupCommandValidator()
    {
        RuleFor(x => x.GroupId)
          .GreaterThan(0).WithMessage("GroupId must be positive.");
        RuleFor(x => x.MemberId)
          .GreaterThan(0).WithMessage("MemberId must be positive.");
    }
}
