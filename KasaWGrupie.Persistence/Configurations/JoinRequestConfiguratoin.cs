using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
	public void Configure(EntityTypeBuilder<JoinRequest> builder)
	{
		builder.HasKey(jr => jr.Id);

		builder.Property(jr => jr.RequestingUserId)
			.IsRequired();

		builder.Property(jr => jr.GroupId)
			.IsRequired();

		builder.Property(jr => jr.Status)
			.IsRequired();

		builder.HasOne(jr => jr.RequestingUser)
			.WithMany()
			.HasForeignKey(jr => jr.RequestingUserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(jr => jr.Group)
			.WithMany(g => g.JoinRequests)
			.HasForeignKey(jr => jr.GroupId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

