namespace KasaWGrupie.API;

public static class DependencyInjection
{
	public static IServiceCollection RegisterConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
	{
		//services.Configure<SecretClass>(configuration.GetSection(SecretClass.SectionName));
		return services;
	}
}