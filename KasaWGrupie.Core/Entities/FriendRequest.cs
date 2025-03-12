using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

class FriendRequest : EntityBase
{
	public required User Sender { get; set; }
	public required User Reciever { get; set; }
	public FriendRequestStatus Status { get; set; }
}