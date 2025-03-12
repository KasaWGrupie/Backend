namespace KasaWGrupie.Core.Entities;

class ExpenseSplitRecord : EntityBase
{
	public required User OwingPerson { get; set; }
	public decimal Amount { get; set; }
	public decimal Percentage { get; set; }
	public required ExpenseSplit ExpenseSplit { get; set; }
}
