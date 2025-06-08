using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.MoneyTransfers;

public class GetMoneyTransfersBySenderSpecification : Specification<MoneyTransfer>
{
    public GetMoneyTransfersBySenderSpecification(int senderId)
    {
        Query.Where(mt => mt.SenderId == senderId);
    }
}