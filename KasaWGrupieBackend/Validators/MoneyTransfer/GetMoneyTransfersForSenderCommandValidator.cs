using FluentValidation;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.MoneyTransfer;

public class GetMoneyTransfersForSenderCommandValidator : AbstractValidator<GetMoneyTransferForSenderCommand>
{
    public GetMoneyTransfersForSenderCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0).WithMessage("SenderId is required");
        
        RuleFor(x => x.Status)
            .IsEnumName(typeof(MoneyTransferStatus), caseSensitive: false)
            .WithMessage("Status must be a valid status value")
            .When(x => x.Status != null);
    }
}