using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Users;

public class GetUserByIdWithFriendsSpecification : Specification<User>
{
	public GetUserByIdWithFriendsSpecification(int userId)
	{
		Query
			.Where(u => u.Id == userId)
			.Include(u => u.Friends);
	}
}