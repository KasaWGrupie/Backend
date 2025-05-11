using KasaWGrupie.Infrastructure.BalanceCalculator;
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;

namespace KasaWGrupie.Tests
{
	[TestClass]
	public class BalanceCalculatorTests
	{
		private BalanceCalculator _calculator;

		[TestInitialize]
		public void Setup()
		{
			_calculator = new BalanceCalculator();
		}

		private Dictionary<int, decimal> CalculateFinalBalance(BalanceResult result)
		{
			var balances = new Dictionary<int, decimal>();

			foreach (var record in result.BalanceRecords)
			{
				if (!balances.ContainsKey(record.FromUserId))
					balances[record.FromUserId] = 0;
				if (!balances.ContainsKey(record.ToUserId))
					balances[record.ToUserId] = 0;

				balances[record.FromUserId] -= record.Amount;
				balances[record.ToUserId] += record.Amount;
			}

			return balances;
		}
		private void AssertBalanceIsZero(Dictionary<int, decimal> balances, int userId)
		{
			if (balances.TryGetValue(userId, out var balance))
			{
				Assert.AreEqual(0, balance, $"User {userId} should have zero balance");
			}
		}

		[TestMethod]
		public void SingleExpense_BalanceShouldBeZero()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>
			{
				new FakeExpenseBalance
				{
					PayingPerson = 1,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 50),
						(2, 50)
					}
				}
			};

			var moneyTransfers = new List<IMoneyTransferBalance>()
			{
				new FakeMoneyTransferBalance
				{
					FromUserId = 2,
					ToUserId = 1,
					Amount = 50
				}
			};

			// Act
			var result = _calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			var balances = CalculateFinalBalance(result);

			AssertBalanceIsZero(balances, 1);
			AssertBalanceIsZero(balances, 2);
		}

		[TestMethod]
		public void MultipleExpenses_BalancesShouldReflectDebts()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>
			{
				new FakeExpenseBalance
				{
					PayingPerson = 1,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 40),
						(2, 40)
					}
				},
				new FakeExpenseBalance
				{
					PayingPerson = 2,
					AmountsPerUser = new List<(int, decimal)>
					{
						(2, 30),
						(1, 30)
					}
				}
			};

			var moneyTransfers = new List<IMoneyTransferBalance>();

			// Act
			var result = _calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			var balances = CalculateFinalBalance(result);

			Assert.AreEqual(10, balances[1]);
			Assert.AreEqual(-10, balances[2]);
		}

		[TestMethod]
		public void ExpenseWithPartialTransfer_BalancesShouldReflectDebts()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>
			{
				new FakeExpenseBalance
				{
					PayingPerson = 1,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 60),
						(2, 60)
					}
				}
			};

			var moneyTransfers = new List<IMoneyTransferBalance>
			{
				new FakeMoneyTransferBalance
				{
					FromUserId = 2,
					ToUserId = 1,
					Amount = 20
				}
			};

			// Act
			var result = _calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			var balances = CalculateFinalBalance(result);

			Assert.AreEqual(balances[1], 40);
			Assert.AreEqual(balances[2], -40);
		}

		[TestMethod]
		public void NoExpensesAndNoTransfers_ReturnsEmptyBalance()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>();
			var moneyTransfers = new List<IMoneyTransferBalance>();

			// Act
			var result = _calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			Assert.AreEqual(0, result.BalanceRecords.Count);
		}

		[TestMethod]
		public void ComplexScenario_MultiplePeople_BalancesShouldReflectDebts()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>
			{
				new FakeExpenseBalance
				{
					PayingPerson = 1,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 30),
						(2, 30),
						(3, 30)
					}
				},
				new FakeExpenseBalance
				{
					PayingPerson = 2,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 20),
						(2, 20),
						(3, 20)
					}
				}
			};

			var moneyTransfers = new List<IMoneyTransferBalance>
			{
				new FakeMoneyTransferBalance
				{
					FromUserId = 3,
					ToUserId = 1,
					Amount = 10
				}
			};

			// Act
			var result = _calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			var balances = CalculateFinalBalance(result);

			Assert.AreEqual(balances[1], 30);
			Assert.AreEqual(balances[2], 10);
			Assert.AreEqual(balances[3], -40);
		}

		[TestMethod]
		public void MegaAdvancedScenario_AllBalancesShouldBeZero()
		{
			// Arrange
			var expenses = new List<IExpenseBalance>
			{
				new FakeExpenseBalance
				{
					PayingPerson = 1,
					AmountsPerUser = new List<(int, decimal)>
					{
						(1, 20),
						(2, 20),
						(3, 20),
						(4, 20),
						(5, 20)
					}
				},
				new FakeExpenseBalance
				{
					PayingPerson = 2,
					AmountsPerUser = new List<(int, decimal)>
					{
						(2, 20),
						(3, 20),
						(4, 20)
					}
				},
				new FakeExpenseBalance
				{
					PayingPerson = 3,
					AmountsPerUser = new List<(int, decimal)>
					{
						(3, 20),
						(4, 20)
					}
				},
				new FakeExpenseBalance
				{
					PayingPerson = 5,
					AmountsPerUser = new List<(int, decimal)>
					{
						(5, 20)
					}
				}
			};

			var moneyTransfers = new List<IMoneyTransferBalance>
			{
				new FakeMoneyTransferBalance
				{
					FromUserId = 2,
					ToUserId = 1,
					Amount = 20
				}
			};

			var calculator = new BalanceCalculator();

			// Act
			var result = calculator.CalculateBalanceInGroup(expenses, moneyTransfers);

			// Assert
			var balances = CalculateFinalBalance(result);

			Assert.AreEqual(balances[1], 60);
			Assert.AreEqual(balances[2], 40);
			Assert.AreEqual(balances[3], -20);
			Assert.AreEqual(balances[4], -60);
			Assert.AreEqual(balances[5], -20);
		}
	}
}
