using FluentValidation;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class UpdateMoneyRequestStatusCommandValidator : AbstractValidator<UpdateMoneyRequestStatusCommand>
{
    public UpdateMoneyRequestStatusCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage("RequestId is required");
        
        RuleFor(x => x.UpdateMoneyRequestStatusDto)
            .SetValidator(new UpdateMoneyRequestStatusDtoValidator());
    }
}