using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;

namespace KasaWGrupie.Infrastructure.BalanceCalculator;

public class BalanceCalculator : IGroupBalanceCalculator
{
	public BalanceResult CalculateBalanceInGroup(List<IExpenseBalance> expenses, List<IMoneyTransferBalance> moneyTransfers)
	{
		var balances = new Dictionary<int, decimal>();

		foreach (var expense in expenses)
		{
			var payingPerson = expense.PayingPerson;
			if (!balances.ContainsKey(payingPerson))
			{
				balances[payingPerson] = 0;
			}

			foreach (var (userId, amount) in expense.GetAmountPerUser())
			{
				if (!balances.ContainsKey(userId))
				{
					balances[userId] = 0;
				}
				balances[userId] -= amount;
				balances[payingPerson] += amount;
			}
		}

		foreach (var transfer in moneyTransfers)
		{
			var fromUser = transfer.FromUserId;
			var toUser = transfer.ToUserId;
			var amount = transfer.Amount;
			if (!balances.ContainsKey(fromUser))
			{
				balances[fromUser] = 0;
			}
			if (!balances.ContainsKey(toUser))
			{
				balances[toUser] = 0;
			}
			balances[fromUser] += amount;
			balances[toUser] -= amount;
		}

		var owedUsers = new List<(int, decimal)>();
		var usersInDebt = new List<(int, decimal)>();

		foreach (var (userId, balance) in balances)
		{
			if (balance > 0)
			{
				owedUsers.Add((userId, balance));
			}
			else if (balance < 0)
			{
				usersInDebt.Add((userId, -balance));
			}
		}

		owedUsers.Sort((x, y) => x.Item2.CompareTo(y.Item2));
		usersInDebt.Sort((x, y) => x.Item2.CompareTo(y.Item2));

		var balanceRecords = new List<BalanceRecord>();

		SettleDebt(balanceRecords, owedUsers, usersInDebt);

		return new BalanceResult { BalanceRecords = balanceRecords };
	}

	private void SettleDebt(List<BalanceRecord> balanceRecords, List<(int UserId, decimal Amount)> owedUsers, List<(int UserId, decimal Amount)> usersInDebt)
	{
		while (usersInDebt.Count > 0)
		{
			(int UserId, decimal Amount) userInDebt = usersInDebt[0];
			(int UserId, decimal Amount)? owedUser = null;

			foreach ((int UserId, decimal Amount) owed in owedUsers)
			{
				if (owed.Amount >= userInDebt.Amount)
				{
					owedUser = owed;
					break;
				}
			}

			if (owedUser != null)
			{
				balanceRecords.Add(new BalanceRecord
				{
					FromUserId = userInDebt.UserId,
					ToUserId = owedUser.Value.UserId,
					Amount = userInDebt.Amount
				});

				if (owedUser.Value.Amount == userInDebt.Amount)
				{
					owedUsers.Remove(owedUser.Value);
				}
				else
				{
					owedUsers[owedUsers.IndexOf(owedUser.Value)] = (owedUser.Value.UserId, owedUser.Value.Amount - userInDebt.Amount);
					owedUsers.Sort((x, y) => x.Amount.CompareTo(y.Item2));
				}

				usersInDebt.Remove(userInDebt);
			}

			else
			{
				balanceRecords.Add(new BalanceRecord
				{
					FromUserId = userInDebt.UserId,
					ToUserId = owedUsers[owedUsers.Count - 1].UserId,
					Amount = owedUsers[owedUsers.Count - 1].Amount
				});

				usersInDebt[0] = (userInDebt.UserId, userInDebt.Amount - owedUsers[owedUsers.Count - 1].Amount);
				usersInDebt.Sort((x, y) => x.Amount.CompareTo(y.Amount));
				owedUsers.RemoveAt(owedUsers.Count - 1);
			}
		}
	}
}
