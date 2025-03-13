using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class PayRequest : EntityBase
{
	public int SenderId { get; set; }
	public required User Sender { get; set; }
	public int ReceiverId { get; set; }
	public required User Receiver { get; set; }
	public decimal Amount { get; set; } //TODO: zastanowić się, czy to pole ma sens
	public ICollection<Group> GroupsToSettle { get; set; } = [];
	public PayRequstStatus payRequstStatus { get; set; }

	//TODO: Może dodać pole message?
}