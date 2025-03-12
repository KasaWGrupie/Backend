using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

class Group : EntityBase
{
	public required string Name { get; set; }
	public required string PictureUrl { get; set; }
	public Currency currency { get; set; }
	public User Admin { get; set; }
	public GroupStatus Status { get; set; }
	public ICollection<User> Members { get; set; }
	public ICollection<Expence> Expences { get; set; }
	public ICollection<MoneyTransfer> MoneyTransfers { get; set; }
	public ICollection<JoinRequest> JoinRequests { get; set; }
}