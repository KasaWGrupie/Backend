using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.MoneyTransfer;

public class UpdateMoneyTransferDtoValidator : AbstractValidator<UpdateMoneyTransferDto>
{
    public UpdateMoneyTransferDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .IsEnumName(typeof(MoneyTransferStatus), caseSensitive: false)
            .WithMessage("Invalid status");
    }

}