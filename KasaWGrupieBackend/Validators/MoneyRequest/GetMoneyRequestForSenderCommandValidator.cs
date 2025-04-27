using FluentValidation;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class GetMoneyRequestForSenderCommandValidator : AbstractValidator<GetMoneyRequestForSenderCommand>
{
    public GetMoneyRequestForSenderCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0).WithMessage("SenderId is required");
    }
}