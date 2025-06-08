using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Requests.Groups;

public class GetGroupByIdCommandValidator
  : AbstractValidator<GetGroupByIdCommand>
{
    public GetGroupByIdCommandValidator()
    {
        RuleFor(x => x.GroupId)
          .GreaterThan(0)
          .WithMessage("GroupId must be greater than zero.");
    }
}
