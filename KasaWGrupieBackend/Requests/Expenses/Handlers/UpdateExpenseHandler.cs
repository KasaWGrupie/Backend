using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using MediatR;

namespace KasaWGrupie.API.Requests.Expenses.Handlers;

public class UpdateExpenseHandler : IRequestHandler<UpdateExpenseCommand, Result>
{
	private readonly IRepositoryBase<Expense> _expenseRepository;
	private readonly IRepositoryBase<ExpenseSplit> _expenseSplitRepository;
	private readonly IRepositoryBase<ExpenseSplitRecord> _expenseSplitRecordRepository;
	
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<UpdateExpenseDto> _validator;

	public UpdateExpenseHandler(IRepositoryBase<Expense> expenseRepository, IRepositoryBase<ExpenseSplit> expenseSplitRepository, IRepositoryBase<ExpenseSplitRecord> expenseSplitRecordRepository, IRepositoryBase<User> userRepository, IValidator<UpdateExpenseDto> validator)
	{
		_expenseRepository = expenseRepository;
		_expenseSplitRepository = expenseSplitRepository;
		_expenseSplitRecordRepository = expenseSplitRecordRepository;
		_userRepository = userRepository;
		_validator = validator;
	}
	
	public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
	{
		var dto = request.UpdateExpenseDto;
		
		var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var expense = await _expenseRepository.GetByIdAsync(dto.ExpenseId, cancellationToken);
		if (expense == null)
		{
			return Result.Invalid(new ValidationError("ExpenseId", "Expense not found"));
		}
		
		User? payingPerson = null;
		if (dto.PaidBy is {} paidBy)
		{
			payingPerson = await _userRepository.GetByIdAsync(paidBy, cancellationToken);
			if (payingPerson == null)
			{
				return Result.Invalid(new ValidationError("PayingPersonId", "Paying person not found"));
			}
		}
		
		ExpenseSplitType? splitType = null;
		if (dto.DivisionMethod != null)
		{
			splitType = Enum.Parse<ExpenseSplitType>(dto.DivisionMethod, true);
		}
		
		List<(User user, decimal amount)>? participants = null;
		if (dto.Participants != null)
		{
			participants = [];
			foreach (var participantDto in dto.Participants)
			{
				var user = await _userRepository.GetByIdAsync(participantDto.UserId, cancellationToken);
				if (user == null)
				{
					return Result.Invalid(new ValidationError("UserId", "User not found"));
				}

				participants.Add((user, participantDto.Amount));
			}
		}

		if (payingPerson != null)
			expense.PayingPerson = payingPerson;
		
		if (dto.ExpenseName != null)
			expense.Name = dto.ExpenseName;
		
		if (dto.ExpensePictureUri != null)
			expense.PictureUrl = dto.ExpensePictureUri;
		
		if (dto.Description != null)
			expense.Description = dto.Description;
		
		if (dto.Amount is {} amount)
			expense.Amount = amount;
		
		if (dto.Date is {} date)
			expense.Date = date;
		
		var expenseSplit = expense.ExpenseSplit;
		
		if (splitType is {} type)
			expenseSplit.Type = type;
		
		if (participants != null)
		{
			var oldRecords = expenseSplit.SplitRecords;
			var newRecords = participants.Select(participant =>
				new ExpenseSplitRecord
				{
					ExpenseSplit = expenseSplit,
					OwingPerson = participant.user,
					Amount = participant.amount,
				}).ToList();
			
			expenseSplit.SplitRecords = newRecords;
		
			await _expenseSplitRecordRepository.DeleteRangeAsync(oldRecords, cancellationToken);
			await _expenseSplitRecordRepository.AddRangeAsync(newRecords, cancellationToken);
			await _expenseSplitRecordRepository.SaveChangesAsync(cancellationToken);
		}
				
		await _expenseRepository.UpdateAsync(expense, cancellationToken);
		await _expenseSplitRepository.UpdateAsync(expenseSplit, cancellationToken);
		
		await _expenseRepository.SaveChangesAsync(cancellationToken);
		await _expenseSplitRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}