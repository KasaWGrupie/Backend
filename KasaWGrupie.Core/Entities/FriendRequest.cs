using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class FriendRequest : EntityBase
{
	public int SenderId { get; set; }
	public required User Sender { get; set; }
	public int ReceiverId { get; set; }
	public required User Receiver { get; set; }
	public FriendRequestStatus Status { get; set; }
}