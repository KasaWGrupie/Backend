using FluentValidation;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;

namespace KasaWGrupie.API.Validators.MoneyTransfer;

public class UpdateMoneyTransferCommandValidator : AbstractValidator<UpdateMoneyTransferCommand>
{
    public UpdateMoneyTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .GreaterThan(0).WithMessage("TransferId is required");
        
        RuleFor(x => x.UpdateMoneyTransferDto)
            .SetValidator(new UpdateMoneyTransferDtoValidator());
    }
}