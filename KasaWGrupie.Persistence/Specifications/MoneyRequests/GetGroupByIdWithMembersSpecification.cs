using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.MoneyRequests;

public class GetGroupByIdWithMembersSpecification : Specification<Group>
{
    public GetGroupByIdWithMembersSpecification(int groupId)
    {
        Query.Include(g => g.Members)
            .Where(g => g.Id == groupId);
    }
}