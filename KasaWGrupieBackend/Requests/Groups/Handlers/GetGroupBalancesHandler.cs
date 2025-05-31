// File: KasaWGrupie.API/Requests/Groups/Handlers/GetGroupBalancesHandler.cs

using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.BalanceCalculator;
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
using KasaWGrupie.Persistence.Specifications.Groups;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.API.Requests.Groups.Handlers
{
    public class GetGroupBalancesHandler
        : IRequestHandler<GetGroupBalancesCommand, Result<GetGroupBalancesDto>>
    {
        private readonly IRepositoryBase<Group> _groupRepository;
        private readonly IRepositoryBase<Expense> _expenseRepository;
        private readonly IRepositoryBase<MoneyTransfer> _moneyTransferRepository;
        private readonly IGroupBalanceCalculator _balanceCalculator;
        private readonly IValidator<GetGroupBalancesCommand> _validator;

        public GetGroupBalancesHandler(
            IRepositoryBase<Group> groupRepository,
            IRepositoryBase<Expense> expenseRepository,
            IRepositoryBase<MoneyTransfer> moneyTransferRepository,
            IGroupBalanceCalculator balanceCalculator,
            IValidator<GetGroupBalancesCommand> validator)
        {
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
            _moneyTransferRepository = moneyTransferRepository;
            _balanceCalculator = balanceCalculator;
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
            if (group is null)
                return Result.NotFound("Group not found");

            // 3) Load all expenses for this group
            var expenseSpec = new ExpensesByGroupIdSpecification(request.GroupId);
            var expenses = await _expenseRepository.ListAsync(expenseSpec, cancellationToken);

            // 4) Load all money transfers for this group
            var mtSpec = new MoneyTransfersByGroupIdSpecification(request.GroupId);
            var moneyTransfers = await _moneyTransferRepository.ListAsync(mtSpec, cancellationToken);

            // 5) Adapt each entity to the calculator interfaces
            var expenseAdapters = expenses
                .Select(e => (IExpenseBalance)new ExpenseBalanceAdapter(e))
                .ToList();

            var mtAdapters = moneyTransfers
                .Select(mt => (IMoneyTransferBalance)new MoneyTransferBalanceAdapter(mt))
                .ToList();

            // 6) Let the calculator do its work
            var balanceResult = _balanceCalculator.CalculateBalanceInGroup(expenseAdapters, mtAdapters);

            // 7) Map BalanceRecord → BalanceDto
            var balances = balanceResult.BalanceRecords
                .Select(br => new BalanceDto(
                    br.FromUserId,
                    br.ToUserId,
                    (float)br.Amount))
                .ToList();

            var dto = new GetGroupBalancesDto(request.GroupId, balances);
            return Result.Success(dto);
        }
    }
}
