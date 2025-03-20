using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Users;

public class UserByEmailSpecification : Specification<User>
{
	public UserByEmailSpecification(string email)
	{
		Query.Where(u => u.Email == email);
	}
}
