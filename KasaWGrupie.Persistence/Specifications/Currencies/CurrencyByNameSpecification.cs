using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Currencies;

public class CurrencyByNameSpecification : Specification<Currency>
{
	public CurrencyByNameSpecification(string name)
	{
		Query.Where(c => c.Name == name);
	}
}