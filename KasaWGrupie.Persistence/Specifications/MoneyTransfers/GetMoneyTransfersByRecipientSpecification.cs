using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.MoneyTransfers;

public class GetMoneyTransfersByRecipientSpecification : Specification<MoneyTransfer>
{
    public GetMoneyTransfersByRecipientSpecification(int recipientId)
    {
        Query.Where(mt => mt.RecipientId == recipientId);
    }
}