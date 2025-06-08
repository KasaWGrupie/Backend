using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.MoneyRequests;

public class GetMoneyRequestsBySenderSpecification : Specification<PayRequest>
{
    public GetMoneyRequestsBySenderSpecification(int senderId)
    {
        Query.Where(pr => pr.SenderId == senderId);
    }
}