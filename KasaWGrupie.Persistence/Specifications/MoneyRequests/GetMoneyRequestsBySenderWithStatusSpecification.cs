using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Persistence.Specifications.MoneyRequests;

public class GetMoneyRequestsBySenderWithStatusSpecification : Specification<PayRequest>
{
    public GetMoneyRequestsBySenderWithStatusSpecification(int senderId, PayRequestStatus status)
    {
        Query.Where(pr => pr.SenderId == senderId)
            .Where(pr => pr.PayRequestStatus == status);
    }
}