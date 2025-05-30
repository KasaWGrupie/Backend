using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;  // for Include()
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups
{
    /// <summary>
    /// Loads a Group by its Id, including the Admin, Currency and Members.
    /// </summary>
    public class GroupByIdWithMembersSpec : Specification<Group>
    {
        public GroupByIdWithMembersSpec(int groupId)
        {
            Query
                .Where(g => g.Id == groupId)
                .Include(g => g.Admin)
                .Include(g => g.Currency)
                .Include(g => g.Members);
        }
    }
}
