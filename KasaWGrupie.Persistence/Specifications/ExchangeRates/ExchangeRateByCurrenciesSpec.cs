using Ardalis.Specification;

namespace KasaWGrupie.Persistence.Specifications.ExchangeRates;

public class ExchangeRateByCurrenciesSpec : Specification<ExchangeRate>
{
	public ExchangeRateByCurrenciesSpec(int fromCurrencyId, int toCurrencyId)
	{
		Query
			.Where(r => r.FromCurrencyId == fromCurrencyId && r.ToCurrencyId == toCurrencyId);
	}
}
