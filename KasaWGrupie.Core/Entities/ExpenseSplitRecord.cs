namespace KasaWGrupie.Core.Entities;

public class ExpenseSplitRecord : EntityBase
{
	public int OwingPersonId { get; set; }
	public required User OwingPerson { get; set; }
	public decimal Amount { get; set; }
	public decimal Percentage { get; set; }
	public int ExpenseSplitId { get; set; }
	public required ExpenseSplit ExpenseSplit { get; set; }
}