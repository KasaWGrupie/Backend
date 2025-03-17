using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class PayRequestConfiguration : IEntityTypeConfiguration<PayRequest>
{
	public void Configure(EntityTypeBuilder<PayRequest> builder)
	{
		builder.HasKey(pr => pr.Id);

		builder.Property(pr => pr.SenderId)
			.IsRequired();

		builder.Property(pr => pr.ReceiverId)
			.IsRequired();

		builder.Property(pr => pr.Amount)
			.HasColumnType(ConfigurationConstants.MoneyDecimalPrecision)
			.IsRequired();

		builder.Property(pr => pr.payRequstStatus)
			.IsRequired();

		builder.HasOne(pr => pr.Sender)
			.WithMany(u => u.SentPayRequests)
			.HasForeignKey(pr => pr.SenderId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(pr => pr.Receiver)
			.WithMany(u => u.RecievedPayRequests)
			.HasForeignKey(pr => pr.ReceiverId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(pr => pr.GroupsToSettle)
			.WithMany();
	}
}
