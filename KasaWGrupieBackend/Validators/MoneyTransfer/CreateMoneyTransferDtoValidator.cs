using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.DTOs.MoneyTransfer;

namespace KasaWGrupie.API.Validators.MoneyTransfer;

public class CreateMoneyTransferDtoValidator : AbstractValidator<CreateMoneyTransferDto>
{
    public CreateMoneyTransferDtoValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0).WithMessage("SenderId is required");
        
        RuleFor(x => x.RecipientId)
            .GreaterThan(0).WithMessage("RecipientId is required");
         
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("GroupId is required");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Requested amount cannot be negative");
    }
}