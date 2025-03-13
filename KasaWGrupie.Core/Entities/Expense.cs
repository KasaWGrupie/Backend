namespace KasaWGrupie.Core.Entities;

public class Expense : EntityBase
{
	public int GroupId { get; set; }
	public required Group Group { get; set; }
	public required string PictureUrl { get; set; }
	public decimal Amount { get; set; }
	public int PayingPersonId { get; set; }
	public required User PayingPerson { get; set; }
	public int ExpenseSplitId { get; set; }
	public required ExpenseSplit ExpenseSplit { get; set; }
}