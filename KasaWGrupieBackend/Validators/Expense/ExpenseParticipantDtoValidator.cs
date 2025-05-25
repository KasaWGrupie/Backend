using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;

namespace KasaWGrupie.API.Validators.Expense;

public class ExpenseParticipantDtoValidator : AbstractValidator<ExpenseParticipantDto>
{
    public ExpenseParticipantDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId is required");
        
    }
}