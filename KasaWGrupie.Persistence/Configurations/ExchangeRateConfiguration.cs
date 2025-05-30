using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
	public void Configure(EntityTypeBuilder<ExchangeRate> builder)
	{
		builder.HasKey(er => er.Id);

		builder.Property(er => er.Rate)
			.IsRequired()
			.HasPrecision(18, 6);

		builder.HasOne(er => er.FromCurrency)
			.WithMany()
			.HasForeignKey(er => er.FromCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(er => er.ToCurrency)
			.WithMany()
			.HasForeignKey(er => er.ToCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasIndex(er => new { er.FromCurrencyId, er.ToCurrencyId })
			.IsUnique();
	}
}
