using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace KasaWGrupie.Persistence.Data;

public sealed class KasaWGrupieDbContext : DbContext
{
	public KasaWGrupieDbContext(DbContextOptions<KasaWGrupieDbContext> options)
	: base(options)
	{
	}

	public DbSet<Currency> Currencies { get; set; }
	public DbSet<Expense> Expenses { get; set; }
	public DbSet<ExpenseSplit> ExpenseSplits { get; set; }
	public DbSet<ExpenseSplitRecord> ExpenseSplitRecords { get; set; }
	public DbSet<FriendRequest> FriendRequests { get; set; }
	public DbSet<Group> Groups { get; set; }
	public DbSet<JoinRequest> JoinRequests { get; set; }
	public DbSet<MoneyTransfer> MoneyTransfers { get; set; }
	public DbSet<PayRequest> PayRequests { get; set; }
	public DbSet<User> Users { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfiguration(new UserConfiguration());
		modelBuilder.ApplyConfiguration(new FriendRequestConfiguration());
		modelBuilder.ApplyConfiguration(new PayRequestConfiguration());
		modelBuilder.ApplyConfiguration(new CurrencyConfiguratoin());
		modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
		modelBuilder.ApplyConfiguration(new ExpenseSplitConfiguration());
		modelBuilder.ApplyConfiguration(new ExpenseSplitRecordConfiguration());
		modelBuilder.ApplyConfiguration(new GroupConfiguration());
		modelBuilder.ApplyConfiguration(new JoinRequestConfiguration());
		modelBuilder.ApplyConfiguration(new MoneyTransferConfiguration());
	}

}
