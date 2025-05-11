using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;

public class FakeExpenseBalance : IExpenseBalance
{
	public int PayingPerson { get; set; }
	public List<(int userId, decimal amount)> AmountsPerUser { get; set; } = new List<(int, decimal)>();

	public IEnumerable<(int UserId, decimal Amount)> GetAmountPerUser()
	{
		return AmountsPerUser;
	}
}

public class FakeMoneyTransferBalance : IMoneyTransferBalance
{
	public int FromUserId { get; set; }
	public int ToUserId { get; set; }
	public decimal Amount { get; set; }
}
