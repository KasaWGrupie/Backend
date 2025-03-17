using DotNetEnv;
using KasaWGrupie.Persistence.Extensions;
using Microsoft.Extensions.Hosting;

namespace KasaWGrupie.API;

public class Program
{
	public static void Main(string[] args)
	{
		Env.Load();

		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.RegisterConfigurationOptions(builder.Configuration);

		builder.Services.ConfigureDbContext(builder.Configuration);

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			using var scope = app.Services.CreateScope();
			scope.ServiceProvider.ApplyMigrations();
		}

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapControllers();

		app.Run();
	}
}

