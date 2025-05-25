using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using MediatR;

namespace KasaWGrupie.API.Requests.Expenses.Handlers;

public class CreateExpenseHandler : IRequestHandler<CreateExpenseCommand, Result>
{
	private readonly IRepositoryBase<Expense> _expenseRepository;
	private readonly IRepositoryBase<ExpenseSplit> _expenseSplitRepository;
	private readonly IRepositoryBase<ExpenseSplitRecord> _expenseSplitRecordRepository;
	private readonly IRepositoryBase<Group> _groupRepository;
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<CreateExpenseDto> _validator;

	public CreateExpenseHandler(IRepositoryBase<Expense> expenseRepository, IRepositoryBase<ExpenseSplit> expenseSplitRepository, IRepositoryBase<ExpenseSplitRecord> expenseSplitRecordRepository, IRepositoryBase<Group> groupRepository, IRepositoryBase<User> userRepository, IValidator<CreateExpenseDto> validator)
	{
		_expenseRepository = expenseRepository;
		_expenseSplitRepository = expenseSplitRepository;
		_expenseSplitRecordRepository = expenseSplitRecordRepository;
		_groupRepository = groupRepository;
		_userRepository = userRepository;
		_validator = validator;
	}

	public async Task<Result> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
	{
		var dto = request.CreateExpenseDto;
		
		var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var group = await _groupRepository.GetByIdAsync(dto.GroupId, cancellationToken);
		if (group == null)
		{
			return Result.Invalid(new ValidationError("GroupId", "Group not found"));
		}
		
		var payingPerson = await _userRepository.GetByIdAsync(dto.PaidBy, cancellationToken);
		if (payingPerson == null)
		{
			return Result.Invalid(new ValidationError("PayingPersonId", "Paying person not found"));
		}
		
		var splitType = Enum.Parse<ExpenseSplitType>(dto.DivisionMethod, true);
		
		var participants = new List<(User user, decimal amount)>();

		foreach (var participantDto in dto.Participants)
		{
			var user = await _userRepository.GetByIdAsync(participantDto.UserId, cancellationToken);
			if (user == null)
			{
				return Result.Invalid(new ValidationError("UserId", "User not found"));
			}
			
			participants.Add((user, participantDto.Amount));
		}

		var expense = new Expense
		{
			Group = group,
			Name = dto.ExpenseName,
			Description = dto.Description,
			Date = dto.Date,
			PayingPerson = payingPerson,
			PictureUrl = dto.ExpensePictureUri,
			Amount = dto.Amount
		};
		var expenseSplit = new ExpenseSplit
		{
			Expense = expense,
			Type = splitType,
		};
		expenseSplit.SplitRecords = participants.Select(participant =>
		{
			var record = new ExpenseSplitRecord
			{
				ExpenseSplit = expenseSplit,
				OwingPerson = participant.user,
			};

			switch (splitType)
			{
				case ExpenseSplitType.ByPercent:
					record.Percentage = participant.amount;
					break;
				case ExpenseSplitType.Custom:
					record.Amount = participant.amount;
					break;
				case ExpenseSplitType.Equally:
					break;
			}
			
			return record;
		}).ToList();

		expense.ExpenseSplit = expenseSplit;
		
		await _expenseRepository.AddAsync(expense, cancellationToken);
		await _expenseSplitRepository.AddAsync(expenseSplit, cancellationToken);
		await _expenseSplitRecordRepository.AddRangeAsync(expenseSplit.SplitRecords, cancellationToken);
		
		await _expenseRepository.SaveChangesAsync(cancellationToken);
		await _expenseSplitRepository.SaveChangesAsync(cancellationToken);
		await _expenseSplitRecordRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}