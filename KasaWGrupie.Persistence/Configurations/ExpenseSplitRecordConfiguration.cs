using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class ExpenseSplitRecordConfiguration : IEntityTypeConfiguration<ExpenseSplitRecord>
{
	public void Configure(EntityTypeBuilder<ExpenseSplitRecord> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.OwingPersonId)
			.IsRequired();

		builder.Property(e => e.ExpenseSplitId)
			.IsRequired();

		builder.Property(e => e.Amount)
			.HasColumnType(ConfigurationConstants.MoneyDecimalPrecision)
			.IsRequired();

		builder.Property(e => e.Percentage)
			.HasColumnType(ConfigurationConstants.PercentDecimalPrecision)
			.IsRequired();

		builder.HasOne(e => e.OwingPerson)
			.WithMany()
			.HasForeignKey(e => e.OwingPersonId)
			.OnDelete(DeleteBehavior.Cascade)
			.IsRequired();

		builder.HasOne(e => e.ExpenseSplit)
			.WithMany(es => es.SplitRecords)
			.HasForeignKey(e => e.ExpenseSplitId)
			.OnDelete(DeleteBehavior.Cascade)
			.IsRequired();
	}
}
