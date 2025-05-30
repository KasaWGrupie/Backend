using Ardalis.Specification;
using KasaWGrupie.Persistence.Specifications.ExchangeRates;

namespace KasaWGrupie.Infrastructure.CurrencyConverter;

public class CurrencyConverterService
{
	private readonly IRepositoryBase<ExchangeRate> _exchangeRateRepo;

	public CurrencyConverterService(IRepositoryBase<ExchangeRate> exchangeRateRepo)
	{
		_exchangeRateRepo = exchangeRateRepo;
	}

	public async Task<decimal> ConvertAsync(decimal amount, int fromCurrencyId, int toCurrencyId)
	{
		if (fromCurrencyId == toCurrencyId)
			return amount;

		var spec = new ExchangeRateByCurrenciesSpec(fromCurrencyId, toCurrencyId);
		var rate = await _exchangeRateRepo.FirstOrDefaultAsync(spec);

		if (rate == null)
		{
			throw new InvalidOperationException($"No exchange rate found from currency {fromCurrencyId} to {toCurrencyId} 😭");
		}

		return amount * rate.Rate;
	}
}
