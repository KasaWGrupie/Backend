using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups;

public class GetGroupByIdWithMembersExpensesAndTransfersSpecification : Specification<Group>
{
    public GetGroupByIdWithMembersExpensesAndTransfersSpecification(int groupId)
    {
        Query.Include(g => g.Members)
            .Include(g => g.Expenses)
            .Include(g => g.MoneyTransfers)
            .Where(g => g.Id == groupId);
    }
}