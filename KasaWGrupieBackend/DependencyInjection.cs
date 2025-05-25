using KasaWGrupie.Persistence.Options;
using KasaWGrupie.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using FirebaseAdmin;
using KasaWGrupie.Persistence.Repositories;
using KasaWGrupie.Infrastructure.ImageService;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Apis.Auth.OAuth2;
using KasaWGrupie.Infrastructure.AuthService;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;


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

		services.AddTransient<IAuthService, AuthService>();
        services.Configure<GcsImageOptions>(opts =>
        {
            opts.BucketName = configuration["GCP_BUCKET_NAME"];
            opts.CredentialsPath = configuration["GCP_CREDENTIALS_PATH"];
        });

        // 2) Create & register a single StorageClient for the whole app:
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<GcsImageOptions>>().Value!;
            GoogleCredential cred = string.IsNullOrWhiteSpace(opts.CredentialsPath)
                ? GoogleCredential.GetApplicationDefault()
                : GoogleCredential.FromFile(opts.CredentialsPath);
            return StorageClient.Create(cred);
        });
        services.AddTransient<IImageService, GcsImageService>();

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

	public static IServiceCollection ConfigureFirebaseApp(this IServiceCollection services, IConfiguration configuration)
	{
		FirebaseApp.Create(new AppOptions
		{
			Credential = GoogleCredential.GetApplicationDefault(),
			ProjectId = "kasawgrupie"
		});
		return services;
	}

}