using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class PayRequest : EntityBase
{
	public int SenderId { get; set; }
	public required User Sender { get; set; }
	public int ReceiverId { get; set; }
	public required User Receiver { get; set; }
	public decimal Amount { get; set; }
	public ICollection<Group> GroupsToSettle { get; set; } = [];
	public PayRequestStatus PayRequestStatus { get; set; }
	public DateTime? EndDate { get; set; }

	//TODO: Może dodać pole message?
}