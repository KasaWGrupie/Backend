using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class ExpenseSplit : EntityBase
{
	public int ExpenseId { get; set; }
	public required Expense Expense { get; set; }
	public ExpenseSplitType Type { get; set; }
	public ICollection<ExpenseSplitRecord> SplitRecords { get; set; } = [];
}