using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupieBackend.Controllers;

namespace KasaWGrupie.API.Validators.MoneyTransfer;

public class UpdateMoneyTransferDtoValidator : AbstractValidator<UpdateMoneyTransferDto>
{
    public UpdateMoneyTransferDtoValidator()
    {
        RuleFor(x => x.TransferId)
            .GreaterThan(0).WithMessage("TransferId is required");
        
        RuleFor(x => x.Status)
            .Must(BeAValidStatus).WithMessage("Invalid status");
    }

    private bool BeAValidStatus(string status)
    {
        var validNames = Enum.GetNames<MoneyTransferStatus>().Select(it => it.ToLower()).ToList();
        return validNames.Contains(status);
    }
}