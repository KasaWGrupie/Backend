using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Requests.Groups;

public class ChangeGroupStatusCommandValidator
  : AbstractValidator<ChangeGroupStatusCommand>
{
    public ChangeGroupStatusCommandValidator()
    {
        RuleFor(x => x.StatusDto.GroupId)
          .GreaterThan(0)
          .WithMessage("GroupId must be greater than zero.");

        RuleFor(x => x.StatusDto.Status)
          .IsInEnum()
          .WithMessage("Status must be a valid GroupStatus.");
    }
}
