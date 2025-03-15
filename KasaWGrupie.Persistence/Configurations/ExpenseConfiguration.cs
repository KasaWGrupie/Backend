using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations
{
	class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
	{
		public void Configure(EntityTypeBuilder<Expense> builder)
		{
			builder.HasKey(e => e.Id);

			builder.Property(e => e.GroupId)
				.IsRequired();

			builder.Property(e => e.PictureUrl)
				.IsRequired()
				.HasMaxLength(ConfigurationConstants.ExpenseConstants.PictureUrlMaxLength);

			builder.Property(e => e.Amount)
				.IsRequired()
				.HasColumnType(ConfigurationConstants.MoneyDecimalPrecision);

			builder.Property(e => e.PayingPersonId)
				.IsRequired();

			builder.Property(e => e.ExpenseSplitId)
				.IsRequired();

			builder.HasOne(e => e.PayingPerson)
				.WithMany()
				.HasForeignKey(e => e.PayingPersonId)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();

			builder.HasOne(e => e.Group)
				.WithMany(g => g.Expenses)
				.HasForeignKey(e => e.GroupId)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();

			builder.HasOne(e => e.ExpenseSplit)
				.WithOne(es => es.Expense)
				.HasForeignKey<Expense>(e => e.ExpenseSplitId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
