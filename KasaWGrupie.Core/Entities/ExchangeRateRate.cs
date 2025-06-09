using KasaWGrupie.Core.Entities;

public class ExchangeRate : EntityBase
{
	public required int FromCurrencyId { get; set; }
	public Currency FromCurrency { get; set; } = null!;

	public required int ToCurrencyId { get; set; }
	public Currency ToCurrency { get; set; } = null!;

	public decimal Rate { get; set; }
}