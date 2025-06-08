using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class GetUserBalancesCommandValidator
  : AbstractValidator<GetUserBalancesCommand>
{
    public GetUserBalancesCommandValidator()
    {
        RuleFor(x => x.UserId)
          .GreaterThan(0)
          .WithMessage("UserId must be a positive integer.");
    }
}
