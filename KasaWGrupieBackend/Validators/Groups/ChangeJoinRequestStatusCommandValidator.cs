using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Requests.Groups;

public class ChangeJoinRequestStatusCommandValidator
  : AbstractValidator<ChangeJoinRequestStatusCommand>
{
    public ChangeJoinRequestStatusCommandValidator()
    {
        RuleFor(x => x.GroupId)
          .GreaterThan(0)
          .WithMessage("GroupId must be a positive integer.");

        RuleFor(x => x.RequestId)
          .GreaterThan(0)
          .WithMessage("RequestId must be a positive integer.");

        RuleFor(x => x.Status)
          .Must(s => s == JoinRequestStatus.Confirmed || s == JoinRequestStatus.Rejected)
          .WithMessage("Status must be either Confirmed or Rejected.");
    }
}
