using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.API.Requests.MoneyRequest.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Infrastructure.BalanceCalculator;
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
using KasaWGrupie.Infrastructure.CurrencyConverter;
using KasaWGrupie.Tests.Factories;
using Moq;

namespace KasaWGrupie.Tests;

[TestClass]
public class CreateMoneyRequestHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IRepositoryBase<PayRequest>> _payRequestRepositoryMock;
	private Mock<IRepositoryBase<Currency>> _currencyRepositoryMock;
	private Mock<IValidator<CreateMoneyRequestDto>> _validatorMock;
	private CreateMoneyRequestHandler _handler;
	private Mock<IGroupBalanceCalculator> _balanceCalculatorMock;
	private Mock<ICurrencyConverter> _currencyConverterMock;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_payRequestRepositoryMock = new Mock<IRepositoryBase<PayRequest>>();
		_currencyRepositoryMock = new Mock<IRepositoryBase<Currency>>();
		_validatorMock = new Mock<IValidator<CreateMoneyRequestDto>>();
		_balanceCalculatorMock = new Mock<IGroupBalanceCalculator>();
		_currencyConverterMock = new Mock<ICurrencyConverter>();

		_handler = new CreateMoneyRequestHandler(
			_userRepositoryMock.Object,
			_groupRepositoryMock.Object,
			_payRequestRepositoryMock.Object,
			_currencyRepositoryMock.Object,
			_validatorMock.Object,
			_balanceCalculatorMock.Object,
			_currencyConverterMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
	{
		// Arrange
		var sender = UserFactory.Create();
		var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
		var currency = new Currency { Name = "USD" };
		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = currency,
			Admin = sender,
			Members = new List<User> { sender, receiver },
			Status = GroupStatus.Active,
			Expenses = new List<Expense>(),
			MoneyTransfers = new List<MoneyTransfer>()
		};

		var dto = new CreateMoneyRequestDto(
			sender.Id,
			receiver.Id,
			"USD",
			new List<int> { group.Id }
		);

		var command = new CreateMoneyRequestCommand(sender.Id, dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiver);
		_groupRepositoryMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);
		_currencyRepositoryMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Currency>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(currency);

		// Setup balance calculator mock
		_balanceCalculatorMock
			.Setup(bc => bc.CalculateBalanceInGroup(
				It.IsAny<List<IExpenseBalance>>(),
				It.IsAny<List<IMoneyTransferBalance>>()))
			.Returns(new BalanceResult { BalanceRecords = new List<BalanceRecord>() });

		_currencyConverterMock
			.Setup(cc => cc.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
			.ReturnsAsync((decimal amount, int _, int _) => amount);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_payRequestRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<PayRequest>(), It.IsAny<CancellationToken>()), Times.Once);
		_payRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		_balanceCalculatorMock.Verify(
			bc => bc.CalculateBalanceInGroup(
				It.IsAny<List<IExpenseBalance>>(),
				It.IsAny<List<IMoneyTransferBalance>>()),
			Times.Once);
	}

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
    {
        // Arrange
        var dto = new CreateMoneyRequestDto(
            1,
            2,
            "USD",
            new List<int> { 1 }
            );
        var command = new CreateMoneyRequestCommand(1, dto);

		var validationFailure = new FluentValidation.Results.ValidationFailure("PropertyName", "Error message");
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.Invalid);
		_payRequestRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<PayRequest>(), It.IsAny<CancellationToken>()), Times.Never);
	}

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenSenderNotFound()
    {
        // Arrange
        var dto = new CreateMoneyRequestDto(
            1, 
            2,
            "USD",
            new List<int> { 1 }
            );
        var command = new CreateMoneyRequestCommand(1, dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(dto.SenderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.NotFound);
	}

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenReceiverNotFound()
    {
        // Arrange
        var sender = UserFactory.Create();
        var dto = new CreateMoneyRequestDto(
            sender.Id,
            2,
            "USD",
            new List<int> { 1 }
            );
        var command = new CreateMoneyRequestCommand(sender.Id, dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(dto.ReceiverId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.NotFound);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnNotFound_WhenGroupNotFound()
	{
		// Arrange
		var sender = UserFactory.Create();
		var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");

        var dto = new CreateMoneyRequestDto(
            sender.Id,
            receiver.Id,
            "USD",
            new List<int> { 1 }
            );
        var command = new CreateMoneyRequestCommand(sender.Id, dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiver);
		_groupRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Group?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.NotFound);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenSenderNotInGroup()
	{
		// Arrange
		var sender = UserFactory.Create();
		var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = receiver,
			Members = new List<User> { receiver },
			Status = GroupStatus.Active
		};

        var dto = new CreateMoneyRequestDto(
            1,
            2,
            "USD",
            new List<int> { group.Id }
            );
        var command = new CreateMoneyRequestCommand(1, dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiver);
		_groupRepositoryMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.Invalid);
	}

	[TestMethod]
	public async Task Handle_ShouldCalculateCorrectAmount_WithExpensesAndTransfers()
	{
		// Arrange
		var sender = UserFactory.Create(id: 1);
		var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
		var currency = new Currency { Id = 1, Name = "USD" };

        
        var group = new Group
        {
	        Id = 1,
	        Name = "Group 1",
	        Description = "Group 1 description",
	        PictureUrl = "pic.jpg",
	        Currency = currency,
	        Admin = sender,
	        AdminId = sender.Id,
	        Members = new List<User> { sender, receiver },
	        Status = GroupStatus.Active,
        };

		var expense = new Expense
		{
			Id = 1,
			Amount = 100,
			Group = group,
			GroupId = group.Id,
			PayingPerson = sender,
			PayingPersonId = sender.Id,
			Name = "Test expense",
			Description = "Test expense description",
			PictureUrl = "pic.jpg"
		};
		expense.ExpenseSplit = new ExpenseSplit
		{
			Expense = expense,
			ExpenseId = expense.Id,
			Type = ExpenseSplitType.Equally
		};
		expense.ExpenseSplit.SplitRecords = new List<ExpenseSplitRecord>
		{
			new ExpenseSplitRecord()
			{
				ExpenseSplit = expense.ExpenseSplit,
				ExpenseSplitId = expense.ExpenseSplit.Id,
				OwingPerson = receiver,
				OwingPersonId = receiver.Id,
			},
			new ExpenseSplitRecord()
			{
				ExpenseSplit = expense.ExpenseSplit,
				ExpenseSplitId = expense.ExpenseSplit.Id,
				OwingPerson = sender,
				OwingPersonId = sender.Id,
			}
		};

		group.Expenses = new List<Expense> { expense };

		group.MoneyTransfers = new List<MoneyTransfer>
		{
			new MoneyTransfer
			{
				Id = 1,
				Amount = 30,
				Group = group,
				GroupId = group.Id,
				Recipient = sender,
				RecipientId = sender.Id,
				Sender = receiver,
				SenderId = receiver.Id,
			}
		};


		var dto = new CreateMoneyRequestDto(
			sender.Id,
			receiver.Id,
			"USD",
			new List<int> { group.Id }
		);

        var command = new CreateMoneyRequestCommand(sender.Id, dto);

		// Mock balance calculation result
		var balanceRecords = new List<BalanceRecord>
		{
			new BalanceRecord
			{
				FromUserId = receiver.Id,
				ToUserId = sender.Id,
				Amount = 20M // Receiver still owes sender 20 after the transfer
            }
		};

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiver);
		_groupRepositoryMock.Setup(r =>
				r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);
		_currencyRepositoryMock.Setup(r =>
				r.FirstOrDefaultAsync(It.IsAny<ISpecification<Currency>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(currency);

		_balanceCalculatorMock
			.Setup(bc => bc.CalculateBalanceInGroup(
				It.IsAny<List<IExpenseBalance>>(),
				It.IsAny<List<IMoneyTransferBalance>>()))
			.Returns(new BalanceResult { BalanceRecords = balanceRecords });
		_currencyConverterMock
			.Setup(cc => cc.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
			.ReturnsAsync((decimal amount, int _, int _) => amount);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _payRequestRepositoryMock.Verify(repo => repo.AddAsync(
                It.Is<PayRequest>(pr =>
                    pr.Amount == 20M && // Verify the calculated amount
                    pr.Sender == sender &&
                    pr.Receiver == receiver),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify that balance calculator was called with correct data
        _balanceCalculatorMock.Verify(
            bc => bc.CalculateBalanceInGroup(
                It.Is<List<IExpenseBalance>>(e => e.Count == group.Expenses.Count),
                It.Is<List<IMoneyTransferBalance>>(t => t.Count == group.MoneyTransfers.Count)),
            Times.Once);
	}
	
	
	[TestMethod]
	public async Task Handle_ShouldReturnForbidden_WhenCommandIdNotSenderId()
	{
		// Arrange
		var sender = UserFactory.Create();
		var dto = new CreateMoneyRequestDto(
			sender.Id,
			2,
			"USD",
			new List<int> { 1 }
		);
		var command = new CreateMoneyRequestCommand(5, dto);
	
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());
			
		// Act
		var result = await _handler.Handle(command, CancellationToken.None);
	
		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.Forbidden);
	}
	
}