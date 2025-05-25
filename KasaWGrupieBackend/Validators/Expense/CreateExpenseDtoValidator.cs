using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.Validators.Expense;

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("GroupId is required");
        
        RuleFor(x => x.ExpenseName)
            .NotEmpty().WithMessage("ExpenseName is required")
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.NameMaxLength)
            .WithMessage($"ExpenseName cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.NameMaxLength} characters");
        
        RuleFor(x => x.ExpensePictureUri)
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.PictureUrlMaxLength)
            .WithMessage($"PictureUri cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.PictureUrlMaxLength} characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(ValidatorConstants.CreateExpenseDtoConstants.DescriptionMaxLength)
            .WithMessage($"Description cannot exceed {ValidatorConstants.CreateExpenseDtoConstants.DescriptionMaxLength} characters");
        
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative");

        RuleForEach(x => x.Participants)
            .SetValidator(new ExpenseParticipantDtoValidator());
        
        RuleFor(x => x.DivisionMethod)
            .NotEmpty().WithMessage("DivisionMethod is required")
            .IsEnumName(typeof(ExpenseSplitType), caseSensitive: false)
            .WithMessage("Invalid DivisionMethod");

        // Percentage split validation
        When(x => x.DivisionMethod.Equals(ExpenseSplitType.ByPercent.ToString(), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Participants)
                .Must(participants => participants.All(p => p.Amount is >= 0 and <= 1))
                .WithMessage("All percentages must be between 0 and 1")
                .Must(participants => Math.Abs(participants.Sum(p => p.Amount) - 1) < 0.0001M)
                .WithMessage("Sum of percentages must equal 100%");
        });

        // Custom split validation
        When(x => x.DivisionMethod.Equals(ExpenseSplitType.Custom.ToString(), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Participants)
                .Must(participants => participants.All(p => p.Amount >= 0))
                .WithMessage("All amounts must be non-negative")
                .Must((dto, participants) => Math.Abs(participants.Sum(p => p.Amount) - dto.Amount) < 0.0001M)
                .WithMessage("Sum of split amounts must equal the total expense amount");
        });
    }
}