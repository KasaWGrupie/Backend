using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Infrastructure.BalanceCalculator;
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
using KasaWGrupie.Persistence.Specifications.Currencies;
using KasaWGrupie.Persistence.Specifications.Groups;
using KasaWGrupie.Infrastructure.CurrencyConverter;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Handlers;

public class CreateMoneyRequestHandler : IRequestHandler<CreateMoneyRequestCommand, Result>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IRepositoryBase<Group> _groupRepository;
	private readonly IRepositoryBase<PayRequest> _payRequestRepository;
	private readonly IRepositoryBase<Currency> _currencyRepository;
	private readonly IValidator<CreateMoneyRequestDto> _validator;

	private readonly IGroupBalanceCalculator _balanceCalculator;
	private readonly ICurrencyConverter _currencyConverter;

	public CreateMoneyRequestHandler(IRepositoryBase<User> userRepository, IRepositoryBase<Group> groupRepository, IRepositoryBase<PayRequest> payRequestRepository, IRepositoryBase<Currency> currencyRepository, IValidator<CreateMoneyRequestDto> validator, IGroupBalanceCalculator balanceCalculator, ICurrencyConverter currencyConverter)
	{
		_userRepository = userRepository;
		_groupRepository = groupRepository;
		_payRequestRepository = payRequestRepository;
		_currencyRepository = currencyRepository;
		_validator = validator;
		_balanceCalculator = balanceCalculator;
		_currencyConverter = currencyConverter;
	}

    public async Task<Result> Handle(CreateMoneyRequestCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateMoneyRequestDto;
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }

        if (request.UserId != dto.SenderId)
        {
            return Result.Forbidden("You are not allowed to create money requests for other users.");
        }
        
        var sender = await _userRepository.GetByIdAsync(dto.SenderId, cancellationToken);
        if (sender == null)
        {
            return Result.NotFound("Sender user not found");
        }
        
        var receiver = await _userRepository.GetByIdAsync(dto.ReceiverId, cancellationToken);
        if (receiver == null)
        {
            return Result.NotFound("Receiver user not found");
        }
        
        var groups = new List<Group>();
        foreach (var groupId in dto.Groups)
        {
            var specification = new GetGroupByIdWithMembersExpensesAndTransfersSpecification(groupId);
            var group = await _groupRepository.FirstOrDefaultAsync(specification, cancellationToken);
            if (group == null)
            {
                return Result.NotFound("Group not found");
            }

			if (!group.Members.Contains(sender))
			{
				return Result.Invalid(new ValidationError("SenderId", $"Sender user is not a member of the group {groupId}"));
			}
			if (!group.Members.Contains(receiver))
			{
				return Result.Invalid(new ValidationError("ReceiverId", $"Receiver user is not a member of the group {groupId}"));
			}

			groups.Add(group);
		}

		var currencySpec = new CurrencyByNameSpecification(dto.Currency);
		var currency = await _currencyRepository.FirstOrDefaultAsync(currencySpec, cancellationToken);

		if (currency == null)
		{
			return Result.NotFound("Currency not found");
		}


		var payRequest = new PayRequest
		{
			Sender = sender,
			Receiver = receiver,
			GroupsToSettle = groups,
			Amount = await CalculateAmount(sender, receiver, groups, currency),
			Currency = currency,
			PayRequestStatus = PayRequestStatus.Pending
		};

		await _payRequestRepository.AddAsync(payRequest, cancellationToken);
		await _payRequestRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}

	private async Task<decimal> CalculateAmount(User sender, User receiver, List<Group> groups, Currency currency)
	{
		var totalAmount = 0M;
		foreach (var group in groups)
		{
			var expenses = group.Expenses.Select(e => new ExpenseBalanceAdapter(e)).ToList<IExpenseBalance>();
			var transfers = group.MoneyTransfers.Select(t => new MoneyTransferBalanceAdapter(t)).ToList<IMoneyTransferBalance>();
			var result = _balanceCalculator.CalculateBalanceInGroup(expenses, transfers);

			var groupAmount = 0M;
			foreach (var record in result.BalanceRecords)
			{
				if (record.FromUserId == sender.Id && record.ToUserId == receiver.Id)
				{
					groupAmount += await _currencyConverter.ConvertAsync(record.Amount, group.CurrencyId, currency.Id);
				}
				else if (record.FromUserId == receiver.Id && record.ToUserId == sender.Id)
				{
					groupAmount -= await _currencyConverter.ConvertAsync(record.Amount, group.CurrencyId, currency.Id);
				}
			}

			totalAmount += groupAmount;
		}

		return totalAmount;
	}
}