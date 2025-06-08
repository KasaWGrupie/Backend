using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups
{
	public class GroupsByMemberEmailSpecification : Specification<Group>
	{
		public GroupsByMemberEmailSpecification(string userEmail)
		{
			Query
				.Include(g => g.Members)
				.Include(g => g.Currency)
				.Include(g => g.Admin)
				.Where(g => g.Members.Any(m => m.Email == userEmail));
		}
	}

}
