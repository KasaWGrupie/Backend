using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Groups;
using MediatR;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class GetExpensesHandler : IRequestHandler<GetExpensesCommand, Result<ICollection<GetExpensesDto>>>
{
    private readonly IRepositoryBase<Expense> _expenseRepository;
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IValidator<GetExpensesCommand> _validator;

    public GetExpensesHandler(IRepositoryBase<Expense> expenseRepository, IRepositoryBase<Group> groupRepository, IValidator<GetExpensesCommand> validator)
    {
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
        _validator = validator;
    }

    public async Task<Result<ICollection<GetExpensesDto>>> Handle(GetExpensesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)));
        }

        if (await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken) == null)
        {
            return Result.NotFound("Group not found");
        }
        
        var specification = new ExpensesByGroupIdSpecification(request.GroupId);
        var expenses = await _expenseRepository.ListAsync(specification, cancellationToken);
        
        var expensesDtos = expenses.Select(expense =>
        {
            // TODO zastąpić `default` wartościami z encji jak zostaną dodane
            var participants = expense.ExpenseSplit.SplitRecords.Select(record => new ExpenseParticipantDto(
                record.OwingPersonId,
                default,
                record.Amount
            )).ToList();
            
            return new GetExpensesDto(
                expense.Id,
                default,
                default,
                expense.PictureUrl,
                default,
                expense.Amount,
                default,
                participants,
                expense.ExpenseSplit.Type.ToString()
            );
        }).ToList();
        
        return Result.Success<ICollection<GetExpensesDto>>(expensesDtos);
    }
}