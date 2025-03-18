using KasaWGrupie.Persistence.Options;
using KasaWGrupie.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace KasaWGrupie.API;

public static class DependencyInjection
{
	public static IServiceCollection RegisterConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ConnectionStringOptions>(configuration.GetSection(ConnectionStringOptions.SectionName));
		return services;
	}
	public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<KasaWGrupieDbContext>(
			options => options.UseNpgsql(configuration["ConnectionString:DbConnection"]));
		return services;
	}

}