using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;  // for Include()
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups;

/// <summary>
/// Loads a single Group (by ID) along with its Members, Admin and Currency.
/// </summary>
public class GroupByIdWithMembersSpec
  : Specification<Group>
{
    public GroupByIdWithMembersSpec(int groupId)
    {
        Query
          .Where(g => g.Id == groupId)
          .Include(g => g.Members)
          .Include(g => g.Admin)
          .Include(g => g.Currency);
    }
}