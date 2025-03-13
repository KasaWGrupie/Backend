using KasaWGrupie.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KasaWGrupie.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.HasKey(u => u.Id);

		builder.Property(u => u.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(u => u.Email)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(u => u.ProfilePictureUrl)
			.IsRequired()
			.HasMaxLength(500);

		builder.HasMany(u => u.Friends)
			.WithMany()
			.UsingEntity(j => j.ToTable("UserFriends"));

		builder.HasMany(u => u.Groups)
			.WithMany()
			.UsingEntity(j => j.ToTable("UserGroups"));

		builder.HasMany(u => u.AdministratedGroups)
			.WithMany()
			.UsingEntity(j => j.ToTable("UserAdministratedGroups"));

		builder.HasMany(u => u.SentPayRequests)
			.WithOne(pr => pr.Sender)
			.HasForeignKey(pr => pr.SenderId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(u => u.RecievedPayRequests)
			.WithOne(pr => pr.Receiver)
			.HasForeignKey(pr => pr.ReceiverId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(u => u.SentFriendRequests)
			.WithOne(fr => fr.Sender)
			.HasForeignKey(fr => fr.SenderId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(u => u.RecievedFriendRequests)
			.WithOne(fr => fr.Receiver)
			.HasForeignKey(fr => fr.ReceiverId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}

