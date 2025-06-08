using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Users;

public class UserWithGroupsSpec : Specification<User>
{
    public UserWithGroupsSpec(int userId)
    {
        Query
          .Where(u => u.Id == userId)
          .Include(u => u.Groups);
    }
}
