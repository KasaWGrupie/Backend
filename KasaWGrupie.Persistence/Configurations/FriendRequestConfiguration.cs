using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
	public void Configure(EntityTypeBuilder<FriendRequest> builder)
	{
		builder.HasKey(fr => fr.Id);

		builder.Property(fr => fr.SenderId)
			.IsRequired();

		builder.Property(fr => fr.ReceiverId)
			.IsRequired();

		builder.Property(fr => fr.Status)
			.IsRequired();

		builder.HasOne(fr => fr.Sender)
			.WithMany(u => u.SentFriendRequests)
			.HasForeignKey(fr => fr.SenderId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(fr => fr.Receiver)
			.WithMany(u => u.RecievedFriendRequests)
			.HasForeignKey(fr => fr.ReceiverId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}