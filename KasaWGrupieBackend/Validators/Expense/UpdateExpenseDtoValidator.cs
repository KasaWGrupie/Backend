using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.Expense;

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.ExpenseId)
            .GreaterThan(0).WithMessage("ExpenseId is required");
        
        RuleFor(x => x.ExpenseName)
            .NotEmpty().WithMessage("ExpenseName is required")
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.NameMaxLength)
            .WithMessage($"ExpenseName cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.NameMaxLength} characters")
            .When(x => x.ExpenseName != null);
        
        RuleFor(x => x.ExpensePictureUri)
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.PictureUrlMaxLength)
            .WithMessage($"PictureUri cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.PictureUrlMaxLength} characters")
            .When(x => x.ExpensePictureUri != null);
        
        RuleFor(x => x.Description)
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.DescriptionMaxLength)
            .WithMessage($"Description cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.DescriptionMaxLength} characters")
            .When(x => x.Description != null);
        
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative")
            .When(x => x.Amount != null);

        RuleForEach(x => x.Participants)
            .SetValidator(new ExpenseParticipantDtoValidator())
            .When(x => x.Participants != null);
        
        RuleFor(x => x.DivisionMethod)
            .NotEmpty().WithMessage("DivisionMethod is required")
            .IsEnumName(typeof(ExpenseSplitType), caseSensitive: false)
            .WithMessage("Invalid DivisionMethod")
            .When(x => x.DivisionMethod != null);
    }

}