using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KasaWGrupie.Persistence.Extensions;

static public class ModelBuilderDataExtensions
{
	public static void AddCurrenciesData(this ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Currency>().HasData(
			new Currency { Id = 1, Name = "PLN" },
			new Currency { Id = 2, Name = "EUR" },
			new Currency { Id = 3, Name = "USD" },
			new Currency { Id = 4, Name = "GBP" }
		);

		modelBuilder.Entity<ExchangeRate>().HasData(
			new ExchangeRate { Id = 1, FromCurrencyId = 1, ToCurrencyId = 2, Rate = 0.22m },
			new ExchangeRate { Id = 2, FromCurrencyId = 1, ToCurrencyId = 3, Rate = 0.25m },
			new ExchangeRate { Id = 3, FromCurrencyId = 1, ToCurrencyId = 4, Rate = 0.19m },

			new ExchangeRate { Id = 4, FromCurrencyId = 2, ToCurrencyId = 1, Rate = 4.55m },
			new ExchangeRate { Id = 5, FromCurrencyId = 2, ToCurrencyId = 3, Rate = 1.12m },
			new ExchangeRate { Id = 6, FromCurrencyId = 2, ToCurrencyId = 4, Rate = 0.85m },

			new ExchangeRate { Id = 7, FromCurrencyId = 3, ToCurrencyId = 1, Rate = 4.00m },
			new ExchangeRate { Id = 8, FromCurrencyId = 3, ToCurrencyId = 2, Rate = 0.89m },
			new ExchangeRate { Id = 9, FromCurrencyId = 3, ToCurrencyId = 4, Rate = 0.76m },

			new ExchangeRate { Id = 10, FromCurrencyId = 4, ToCurrencyId = 1, Rate = 5.10m },
			new ExchangeRate { Id = 11, FromCurrencyId = 4, ToCurrencyId = 2, Rate = 1.17m },
			new ExchangeRate { Id = 12, FromCurrencyId = 4, ToCurrencyId = 3, Rate = 1.32m }
		);
	}
}
