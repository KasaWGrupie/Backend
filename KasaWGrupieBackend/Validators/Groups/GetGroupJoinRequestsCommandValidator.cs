using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Requests.Groups;

public class GetGroupJoinRequestsCommandValidator
  : AbstractValidator<GetGroupJoinRequestsCommand>
{
    public GetGroupJoinRequestsCommandValidator()
    {
        RuleFor(x => x.GroupId)
          .GreaterThan(0)
          .WithMessage("GroupId must be a positive integer");
    }
}
