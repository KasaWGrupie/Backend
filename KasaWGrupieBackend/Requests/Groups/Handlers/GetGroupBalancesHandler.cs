using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Groups;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class GetGroupBalancesHandler
  : IRequestHandler<GetGroupBalancesCommand, Result<GetGroupBalancesDto>>
{
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IRepositoryBase<Expense> _expenseRepository;
    private readonly IValidator<GetGroupBalancesCommand> _validator;

    public GetGroupBalancesHandler(
      IRepositoryBase<Group> groupRepository,
      IRepositoryBase<Expense> expenseRepository,
      IValidator<GetGroupBalancesCommand> validator)
    {
        _groupRepository = groupRepository;
        _expenseRepository = expenseRepository;
        _validator = validator;
    }

    public async Task<Result<GetGroupBalancesDto>> Handle(
      GetGroupBalancesCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Check group exists
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.NotFound("Group not found");

        // 3) Load all expenses for this group
        var spec = new ExpensesByGroupIdSpecification(request.GroupId);
        var expenses = await _expenseRepository.ListAsync(spec, cancellationToken);

        // 4) Aggregate “who owes whom”
        var lookup = new Dictionary<(int from, int to), decimal>();
        foreach (var expense in expenses)
        {
            foreach (var record in expense.ExpenseSplit.SplitRecords)
            {
                var key = (record.OwingPersonId, expense.PayingPersonId);
                lookup[key] = lookup.TryGetValue(key, out var sum)
                  ? sum + record.Amount
                  : record.Amount;
            }
        }

        // 5) Map to DTO (decimal → float)
        var balances = lookup
          .Select(kv => new BalanceDto(
            kv.Key.from,
            kv.Key.to,
            (float)kv.Value
          )).ToList();

        // 6) Return wrapped in GetGroupBalancesDto
        var dto = new GetGroupBalancesDto(request.GroupId, balances);
        return Result.Success(dto);
    }
}
