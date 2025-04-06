using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Users;

public class UserByIdSpecification : Specification<User>
{
	public UserByIdSpecification(int id)
	{
		Query.Where(u => u.Id == id);
	}
}