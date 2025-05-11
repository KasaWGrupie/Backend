using KasaWGrupie.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
public interface IExpenseBalance
{
	IEnumerable<(int UserId, decimal Amount)> GetAmountPerUser();
	int PayingPerson { get; }
}
