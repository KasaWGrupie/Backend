using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.FriendRequests;

public class GetFriendRequestByIdWithUsersSpecification : Specification<FriendRequest>
{
	public GetFriendRequestByIdWithUsersSpecification(int requestId)
	{
		Query
			.Where(fr => fr.Id == requestId)
			.Include(fr => fr.Sender)
			.Include(fr => fr.Receiver);
	}
}