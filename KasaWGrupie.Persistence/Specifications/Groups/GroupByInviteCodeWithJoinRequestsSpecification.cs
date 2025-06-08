using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups;

public class GroupByInviteCodeWithJoinRequestsSpecification : Specification<Group>
{
	public GroupByInviteCodeWithJoinRequestsSpecification(string inviteCode)
	{

		Query
			.Include(g => g.JoinRequests)
			.Where(g => g.InviteCode == inviteCode);
	}
}