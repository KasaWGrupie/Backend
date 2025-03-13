using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class Group : EntityBase
{
	public required string Name { get; set; }
	public required string PictureUrl { get; set; }
	public required string Description { get; set; }
	public int CurrencyId { get; set; }
	public required Currency Currency { get; set; }
	public int AdminId { get; set; }
	public required User Admin { get; set; }
	public GroupStatus Status { get; set; }
	public ICollection<User> Members { get; set; } = [];
	public ICollection<Expense> Expenses { get; set; } = [];
	public ICollection<MoneyTransfer> MoneyTransfers { get; set; } = [];
	public ICollection<JoinRequest> JoinRequests { get; set; } = [];
}