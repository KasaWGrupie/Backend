using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.MoneyRequests;

public class GetMoneyRequestsByReceiverWithStatusSpecification : Specification<PayRequest>
{
    public GetMoneyRequestsByReceiverWithStatusSpecification(int receiverId, PayRequestStatus status)
    {
        Query.Where(pr => pr.ReceiverId == receiverId)
            .Where(pr => pr.PayRequestStatus == status);
    }
}