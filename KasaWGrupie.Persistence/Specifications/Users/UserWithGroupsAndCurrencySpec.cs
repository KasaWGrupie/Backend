// KasaWGrupie.Persistence/Specifications/Users/UserWithGroupsAndCurrencySpec.cs
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Users;

public class UserWithGroupsAndCurrencySpec
  : Specification<User>, ISingleResultSpecification
{
    public UserWithGroupsAndCurrencySpec(int userId)
    {
        Query
          .Where(u => u.Id == userId)
          .Include(u => u.Groups)
            .ThenInclude(g => g.Currency);
    }
}
