namespace KasaWGrupie.Infrastructure.CurrencyConverter;
public interface ICurrencyConverter
{
	public Task<decimal> ConvertAsync(decimal amount, int fromCurrencyId, int toCurrencyId);
}
