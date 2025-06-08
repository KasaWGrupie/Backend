using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.Users
{
	public sealed class GetUserByIdWithActiveAndClosingGroupsSpecification : Specification<User>
	{
		public GetUserByIdWithActiveAndClosingGroupsSpecification(int userId)
		{
			Query
				.Where(user => user.Id == userId)
				.Include(user => user.Groups
					.Where(group => group.Status == GroupStatus.Active || group.Status == GroupStatus.Closing));
		}
	}
}