using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Requests.Groups;

public class AddUserToGroupCommandValidator
  : AbstractValidator<AddUserToGroupCommand>
{
    public AddUserToGroupCommandValidator()
    {
        RuleFor(x => x.GroupId)
          .GreaterThan(0).WithMessage("GroupId must be positive.");
        RuleFor(x => x.MemberId)
          .GreaterThan(0).WithMessage("MemberId must be positive.");
    }
}
