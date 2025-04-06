using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

public class GetFriendRequestByReceiverAndSenderUnconfirmedSpecification : Specification<FriendRequest>
{
	public GetFriendRequestByReceiverAndSenderUnconfirmedSpecification(int senderId, int receiverId)
	{
		Query.Where(fr =>
			fr.Sender.Id == senderId &&
			fr.Receiver.Id == receiverId &&
			fr.Status == FriendRequestStatus.Unconfirmed);
	}
}