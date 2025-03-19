using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using KasaWGrupie.Persistence.Data;

namespace KasaWGrupie.Persistence.Extensions;

public static class MigrationExtensions
{
	public static void ApplyMigrations(this IServiceProvider serviceProvider)
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<KasaWGrupieDbContext>();

		dbContext.Database.Migrate();
	}
}