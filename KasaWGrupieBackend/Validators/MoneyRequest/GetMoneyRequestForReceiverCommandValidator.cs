using FluentValidation;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class GetMoneyRequestForReceiverCommandValidator : AbstractValidator<GetMoneyRequestForReceiverCommand>
{
    public GetMoneyRequestForReceiverCommandValidator()
    {
        RuleFor(x => x.ReceiverId)
            .GreaterThan(0).WithMessage("ReceiverId is required");
    }
}