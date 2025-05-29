using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.MoneyTransfers;

public class GetMoneyTransfersBySenderWithStatusSpecification : Specification<MoneyTransfer>
{
    public GetMoneyTransfersBySenderWithStatusSpecification(int senderId, MoneyTransferStatus status)
    {
        Query.Where(mt => mt.SenderId == senderId)
            .Where(mt => mt.Status == status);
    }
}