using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
	public void Configure(EntityTypeBuilder<Group> builder)
	{
		builder.HasKey(g => g.Id);

		builder.Property(g => g.Name)
			.IsRequired()
			.HasMaxLength(ConfigurationConstants.GroupConstants.NameMaxLength);

		builder.Property(g => g.PictureUrl)
			.IsRequired()
			.HasMaxLength(ConfigurationConstants.GroupConstants.PictureUrlMaxLength);

		builder.Property(g => g.Description)
			.IsRequired()
			.HasMaxLength(ConfigurationConstants.GroupConstants.DescriptionMaxLength);

		builder.Property(g => g.CurrencyId)
			.IsRequired();

		builder.Property(g => g.AdminId)
			.IsRequired();

		builder.Property(g => g.Status)
			.IsRequired();

		builder.HasOne(g => g.Currency)
			.WithMany()
			.HasForeignKey(g => g.CurrencyId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(g => g.Admin)
			.WithMany()
			.HasForeignKey(g => g.AdminId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(g => g.Members)
			.WithMany();

		builder.HasMany(g => g.Expenses)
			.WithOne(e => e.Group)
			.HasForeignKey(e => e.GroupId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(g => g.MoneyTransfers)
			.WithOne(mt => mt.Group)
			.HasForeignKey(mt => mt.GroupId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(g => g.JoinRequests)
			.WithOne(jr => jr.Group)
			.HasForeignKey(jr => jr.GroupId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
