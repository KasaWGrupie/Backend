
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.Groups
{
    /// <summary>
    /// Loads all MoneyTransfer entities for a given group (e.g. those where GroupId == groupId).
    /// </summary>
    public class MoneyTransfersByGroupIdSpecification : Specification<MoneyTransfer>, ISingleResultSpecification
    {
        public MoneyTransfersByGroupIdSpecification(int groupId)
        {
            Query
                .Where(mt => mt.GroupId == groupId)
                // Optionally filter by status, e.g. Confirmed only:
                .Where(mt => mt.Status == MoneyTransferStatus.Confirmed);
        }
    }
}
