using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class MoneyTransferConfiguration : IEntityTypeConfiguration<MoneyTransfer>
{
	public void Configure(EntityTypeBuilder<MoneyTransfer> builder)
	{
		builder.HasKey(mt => mt.Id);

		builder.Property(mt => mt.Amount)
			.HasColumnType(ConfigurationConstants.MoneyDecimalPrecision)
			.IsRequired();

		builder.Property(mt => mt.RecipientId)
			.IsRequired();

		builder.Property(mt => mt.SenderId)
			.IsRequired();

		builder.Property(mt => mt.GroupId)
			.IsRequired();

		builder.Property(mt => mt.Status)
			.IsRequired();

		builder.HasOne(mt => mt.Recipient)
			.WithMany()
			.HasForeignKey(mt => mt.RecipientId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(mt => mt.Sender)
			.WithMany()
			.HasForeignKey(mt => mt.SenderId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(mt => mt.Group)
			.WithMany(g => g.MoneyTransfers)
			.HasForeignKey(mt => mt.GroupId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

