namespace KasaWGrupie.Infrastructure.BalanceCalculator;

public class BalanceRecord
{
	public int FromUserId { get; set; }
	public int ToUserId { get; set; }
	public decimal Amount { get; set; }
}
