using KasaWGrupie.Persistence.Options;
using KasaWGrupie.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using KasaWGrupie.Persistence.Repositories;
using KasaWGrupie.Infrastructure.ImageService;
using FluentValidation;
using FluentValidation.AspNetCore;


namespace KasaWGrupie.API;

public static class DependencyInjection
{
	public static IServiceCollection RegisterConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ConnectionStringOptions>(configuration.GetSection(ConnectionStringOptions.SectionName));
		return services;
	}
	public static IServiceCollection ConfigureData(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<KasaWGrupieDbContext>(
			options => options.UseNpgsql(configuration["ConnectionString:DbConnection"]));
		services.AddScoped(typeof(IRepositoryBase<>), typeof(KasaWGrupieRepository<>));
		return services;
	}
	public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.ConfigureMediatR();

		services.AddTransient<IImageService, DummyImageService>();

		return services;
	}

	public static IServiceCollection ConfigureValidators(this IServiceCollection services)
	{
		services.AddFluentValidationAutoValidation();
		services.AddValidatorsFromAssemblyContaining<Program>();

		return services;
	}

	public static IServiceCollection ConfigureMediatR(this IServiceCollection services)
	{
		services.AddMediatR(configurations =>
		{
			configurations.RegisterServicesFromAssembly(typeof(Program).Assembly);
		});

		return services;
	}
	public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<KasaWGrupieDbContext>(
			options => options.UseNpgsql(configuration["ConnectionString:DbConnection"]));
		return services;
	}

}