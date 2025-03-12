using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

class MoneyTransfer
{
	public decimal Amount { get; set; }
	public required User Recipient { get; set; }
	public required User Sender { get; set; }
	public required Group Group { get; set; }
	public required MoneyTransferStatus Status { get; set; }
}