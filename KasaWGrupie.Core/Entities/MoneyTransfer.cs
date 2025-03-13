using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class MoneyTransfer : EntityBase
{
	public decimal Amount { get; set; }
	public int RecipientId { get; set; }
	public required User Recipient { get; set; }
	public int SenderId { get; set; }
	public required User Sender { get; set; }
	public int GroupId { get; set; }
	public required Group Group { get; set; }
	public MoneyTransferStatus Status { get; set; }
}