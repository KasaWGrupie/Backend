using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

class ExpenseSplit : EntityBase
{
	public required Expense Expense { get; set; }
	public ExpenseSplitType Type { get; set; }
	public ICollection<ExpenseSplitRecord> SplitRecords { get; set; } = [];
}