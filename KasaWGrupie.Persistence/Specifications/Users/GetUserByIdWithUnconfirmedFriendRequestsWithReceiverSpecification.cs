using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

public class GetUserByIdWithUnconfirmedFriendRequestsWithReceiverSpecification : Specification<User>
{
	public GetUserByIdWithUnconfirmedFriendRequestsWithReceiverSpecification(int userId)
	{
		Query
			.Where(u => u.Id == userId)
			.Include(u => u.SentFriendRequests.Where(r => r.Status == FriendRequestStatus.Unconfirmed))
				.ThenInclude(r => r.Receiver);
	}
}