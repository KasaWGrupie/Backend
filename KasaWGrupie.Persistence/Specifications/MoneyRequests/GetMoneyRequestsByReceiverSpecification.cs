using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.MoneyRequests;

public class GetMoneyRequestsByReceiverSpecification : Specification<PayRequest>
{
    public GetMoneyRequestsByReceiverSpecification(int receiverId)
    {
        Query.Where(pr => pr.ReceiverId == receiverId);
    }
}