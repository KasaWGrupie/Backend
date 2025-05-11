using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;

class MoneyTransferBalanceAdapter : IMoneyTransferBalance
{
	private MoneyTransfer _moneyTransfer;
	public MoneyTransferBalanceAdapter(MoneyTransfer moneyTransfer)
	{
		_moneyTransfer = moneyTransfer;
	}
	public int FromUserId => _moneyTransfer.SenderId;

	public int ToUserId => _moneyTransfer.RecipientId;

	public decimal Amount => _moneyTransfer.Status == MoneyTransferStatus.Confirmed ? _moneyTransfer.Amount : 0;
}
