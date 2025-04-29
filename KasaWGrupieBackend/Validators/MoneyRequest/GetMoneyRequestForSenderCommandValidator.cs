using FluentValidation;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class GetMoneyRequestForSenderCommandValidator : AbstractValidator<GetMoneyRequestForSenderCommand>
{
    public GetMoneyRequestForSenderCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0).WithMessage("SenderId is required");
        
        RuleFor(x => x.Status)
            .IsEnumName(typeof(PayRequestStatus), caseSensitive: false)
            .WithMessage("Status must be a valid status value")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}