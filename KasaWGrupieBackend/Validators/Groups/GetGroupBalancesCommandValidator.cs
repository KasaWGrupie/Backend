using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups
{
    public class GetGroupBalancesCommandValidator
  : AbstractValidator<GetGroupBalancesCommand>
    {
        public GetGroupBalancesCommandValidator()
        {
            RuleFor(x => x.GroupId)
              .GreaterThan(0).WithMessage("GroupId must be a positive integer");
        }
    }
}
