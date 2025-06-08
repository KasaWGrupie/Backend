using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
public class ExpenseBalanceAdapter : IExpenseBalance
{
	private Expense _expense;
	public ExpenseBalanceAdapter(Expense expense)
	{
		_expense = expense;
	}
	public int PayingPerson => _expense.PayingPersonId;

	public IEnumerable<(int, decimal)> GetAmountPerUser()
	{
		switch (_expense.ExpenseSplit.Type)
		{
			case ExpenseSplitType.Equally:
				return GetAmountPerUserEqually(_expense.ExpenseSplit);
			case ExpenseSplitType.ByPercent:
				return GetAmountPerUserByPercent(_expense.ExpenseSplit);
			case ExpenseSplitType.Custom:
				return GetAmountPerUserCustom(_expense.ExpenseSplit);
			default:
				return Enumerable.Empty<(int, decimal)>();
		}
	}

	private IEnumerable<(int, decimal)> GetAmountPerUserByPercent(ExpenseSplit expenseSplit)
	{
		var result = new List<(int, decimal)>();
		var totalAmount = _expense.Amount;
		foreach (var splitRecord in expenseSplit.SplitRecords)
		{
			result.Add((splitRecord.OwingPersonId, totalAmount * splitRecord.Percentage));
		}
		return result;
	}
	private IEnumerable<(int, decimal)> GetAmountPerUserEqually(ExpenseSplit expenseSplit)
	{
		var result = new List<(int, decimal)>();
		var totalAmount = _expense.Amount;
		int numberOfUsers = expenseSplit.SplitRecords.Count;
		foreach (var splitRecord in expenseSplit.SplitRecords)
		{
			result.Add((splitRecord.OwingPersonId, totalAmount / numberOfUsers));
		}
		return result;
	}
	private IEnumerable<(int, decimal)> GetAmountPerUserCustom(ExpenseSplit expenseSplit)
	{
		var result = new List<(int, decimal)>();
		foreach (var splitRecord in expenseSplit.SplitRecords)
		{
			result.Add((splitRecord.OwingPersonId, splitRecord.Amount));
		}
		return result;
	}
}
