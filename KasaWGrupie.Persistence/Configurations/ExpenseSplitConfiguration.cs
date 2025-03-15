using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
	public void Configure(EntityTypeBuilder<ExpenseSplit> builder)
	{
		builder.HasKey(es => es.Id);

		builder.Property(es => es.ExpenseId)
			.IsRequired();

		builder.HasOne(es => es.Expense)
			.WithOne(es => es.ExpenseSplit)
			.HasForeignKey<ExpenseSplit>(es => es.ExpenseId)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(es => es.Type)
			.IsRequired();

		builder.HasMany(es => es.SplitRecords)
			.WithOne(sr => sr.ExpenseSplit)
			.HasForeignKey(sr => sr.ExpenseSplitId)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);
	}
}