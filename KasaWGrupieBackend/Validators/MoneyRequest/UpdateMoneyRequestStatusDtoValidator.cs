using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.MoneyRequest;

public class UpdateMoneyRequestStatusDtoValidator : AbstractValidator<UpdateMoneyRequestStatusDto>
{
    public UpdateMoneyRequestStatusDtoValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage("RequestId is required");
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .IsEnumName(typeof(PayRequestStatus), caseSensitive: false)
            .WithMessage("Invalid status");
    }
}