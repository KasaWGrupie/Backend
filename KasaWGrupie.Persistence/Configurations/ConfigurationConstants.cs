namespace KasaWGrupie.Persistence.Configurations;

public static class ConfigurationConstants
{
	public const string MoneyDecimalPrecision = "decimal(18,2)";
	public const string PercentDecimalPrecision = "decimal(3,2)";

	public static class CurrencyConstants
	{
		public const int NameMaxLength = 50;
	}

	public static class ExpenseConstants
	{
		public const int PictureUrlMaxLength = 200;
	}
	public static class GroupConstants
	{
		public const int NameMaxLength = 100;
		public const int PictureUrlMaxLength = 200;
		public const int DescriptionMaxLength = 500;
	}
}
