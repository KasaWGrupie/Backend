namespace KasaWGrupie.Persistence.Options;

public sealed class ConnectionStringOptions
{
	public const string SectionName = "ConnectionString";

	public string DbConnection { get; set; } = string.Empty;
}