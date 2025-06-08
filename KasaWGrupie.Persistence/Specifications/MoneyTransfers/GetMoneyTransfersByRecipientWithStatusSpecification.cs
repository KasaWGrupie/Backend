using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.MoneyTransfers;

public class GetMoneyTransfersByRecipientWithStatusSpecification : Specification<MoneyTransfer>
{
    public GetMoneyTransfersByRecipientWithStatusSpecification(int recipientId, MoneyTransferStatus status)
    {
        Query.Where(mt => mt.RecipientId == recipientId)
            .Where(mt => mt.Status == status);
    }
}