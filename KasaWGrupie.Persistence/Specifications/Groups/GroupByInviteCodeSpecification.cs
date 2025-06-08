using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

public class GroupByInviteCodeSpec : Specification<Group>
{
	public GroupByInviteCodeSpec(string inviteCode)
	{
		Query.Where(g => g.InviteCode == inviteCode);
	}
}