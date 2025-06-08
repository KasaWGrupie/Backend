using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

public class GetUserByIdWithUnconfirmedFriendRequestsWithSenderSpecification : Specification<User>
{
	public GetUserByIdWithUnconfirmedFriendRequestsWithSenderSpecification(int userId)
	{
		Query
			.Where(u => u.Id == userId)
			.Include(u => u.RecievedFriendRequests.Where(r => r.Status == FriendRequestStatus.Unconfirmed))
				.ThenInclude(r => r.Sender);
	}
}