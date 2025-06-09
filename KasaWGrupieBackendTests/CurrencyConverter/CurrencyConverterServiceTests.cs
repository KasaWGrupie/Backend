using Moq;
using Ardalis.Specification;
using KasaWGrupie.Infrastructure.CurrencyConverter;

namespace KasaWGrupie.Tests.CurrencyConverter;


[TestClass]
public class CurrencyConverterServiceTests
{
	private Mock<IRepositoryBase<ExchangeRate>> _exchangeRateRepoMock = null!;
	private CurrencyConverterService _service = null!;

	[TestInitialize]
	public void Setup()
	{
		_exchangeRateRepoMock = new Mock<IRepositoryBase<ExchangeRate>>();
		_service = new CurrencyConverterService(_exchangeRateRepoMock.Object);
	}

	[TestMethod]
	public async Task ConvertAsync_SameCurrency_ReturnsSameAmount()
	{
		// Arrange
		decimal amount = 100m;
		int currencyId = 1;

		// Act
		decimal result = await _service.ConvertAsync(amount, currencyId, currencyId);

		// Assert
		Assert.AreEqual(amount, result);
	}

	[TestMethod]
	public async Task ConvertAsync_ValidRate_ReturnsConvertedAmount()
	{
		// Arrange
		decimal amount = 100m;
		int fromCurrencyId = 1;
		int toCurrencyId = 2;
		decimal rate = 0.25m;

		_exchangeRateRepoMock
			.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<ISpecification<ExchangeRate>>(), default))
			.ReturnsAsync(new ExchangeRate
			{
				FromCurrencyId = fromCurrencyId,
				ToCurrencyId = toCurrencyId,
				Rate = rate
			});

		// Act
		decimal result = await _service.ConvertAsync(amount, fromCurrencyId, toCurrencyId);

		// Assert
		Assert.AreEqual(amount * rate, result);
	}

	[TestMethod]
	public async Task ConvertAsync_MissingRate_ThrowsInvalidOperationException()
	{
		// Arrange
		int fromCurrencyId = 1;
		int toCurrencyId = 3;

		_exchangeRateRepoMock
			.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<ISpecification<ExchangeRate>>(), default))
			.ReturnsAsync((ExchangeRate?)null);

		// Act & Assert
		await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
			_service.ConvertAsync(100m, fromCurrencyId, toCurrencyId));
	}
}