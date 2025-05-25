using FluentValidation;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups;

public class GetExpensesCommandValidator : AbstractValidator<GetExpensesCommand>
{
    public GetExpensesCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Group id is required");
    }
}