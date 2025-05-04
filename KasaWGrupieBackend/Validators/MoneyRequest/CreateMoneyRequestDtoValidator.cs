using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class CreateMoneyRequestDtoValidator : AbstractValidator<CreateMoneyRequestDto>
{
    public CreateMoneyRequestDtoValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0).WithMessage("Sender id is required");
        RuleFor(x => x.ReceiverId)
            .GreaterThan(0).WithMessage("Receiver id is required");
        RuleForEach(x => x.Groups)
            .GreaterThan(0).WithMessage("Invalid group id");
    }
}