namespace KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
public interface IMoneyTransferBalance
{
	public int FromUserId { get; }
	public int ToUserId { get; }
	public decimal Amount { get; }
}