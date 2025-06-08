// KasaWGrupie.API/Requests/Users/Validators/GetUserBalancesWithUserCommandValidator.cs
using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class GetUserBalancesWithUserCommandValidator
  : AbstractValidator<GetUserBalancesWithUserCommand>
{
    public GetUserBalancesWithUserCommandValidator()
    {
        RuleFor(x => x.UserId)
          .GreaterThan(0)
          .WithMessage("UserId must be positive.");
        RuleFor(x => x.OtherUserId)
          .GreaterThan(0)
          .WithMessage("OtherUserId must be positive.");
    }
}
