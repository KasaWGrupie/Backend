namespace KasaWGrupie.Core.Entities;

class Expense : EntityBase
{
	public required Group Group { get; set; }
	public required string PictureUrl { get; set; }
	public decimal Amount { get; set; }
	public required User PayingPerson { get; set; }
	public required ExpenseSplit ExpenseSplit { get; set; }


}
