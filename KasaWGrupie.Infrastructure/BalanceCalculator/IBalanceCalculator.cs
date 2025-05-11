using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;

namespace KasaWGrupie.Infrastructure.BalanceCalculator;

public interface IGroupBalanceCalculator
{
	BalanceResult CalculateBalanceInGroup(List<IExpenseBalance> expenses, List<IMoneyTransferBalance> moneyTransfers);
}
