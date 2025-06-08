using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups;

public class JoinRequestsByGroupSpec
  : Specification<JoinRequest>
{
    public JoinRequestsByGroupSpec(int groupId)
    {
        Query
          .Where(j => j.GroupId == groupId);
    }
}
