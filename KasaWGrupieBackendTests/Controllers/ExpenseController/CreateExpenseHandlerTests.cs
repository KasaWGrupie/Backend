using Ardalis.Result;
using Moq;
using KasaWGrupie.Core.Entities;
using FluentAssertions;
using KasaWGrupie.Tests.Factories;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.API.Requests.Expenses.Handlers;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Tests;

[TestClass]
public class CreateExpenseHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<Expense>> _expenseRepositoryMock;
	private Mock<IRepositoryBase<ExpenseSplit>> _expenseSplitRepositoryMock;
	private Mock<IRepositoryBase<ExpenseSplitRecord>> _expenseSplitRecordRepositoryMock;
	private Mock<IValidator<CreateExpenseDto>> _validatorMock;
	private CreateExpenseHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_expenseRepositoryMock = new Mock<IRepositoryBase<Expense>>();
		_expenseSplitRepositoryMock = new Mock<IRepositoryBase<ExpenseSplit>>();
		_expenseSplitRecordRepositoryMock = new Mock<IRepositoryBase<ExpenseSplitRecord>>();
		_validatorMock = new Mock<IValidator<CreateExpenseDto>>();

		_handler = new CreateExpenseHandler(
			_expenseRepositoryMock.Object,
			_expenseSplitRepositoryMock.Object,
			_expenseSplitRecordRepositoryMock.Object,
			_groupRepositoryMock.Object,
			_userRepositoryMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handler_ShouldReturnSuccess_WhenExpenseIsCreatedSuccessfully()
	{
		// Arrange
		var payer = UserFactory.Create();
		var admin = UserFactory.Create(id: 2, email: "admin@example.com");
		var member1 = UserFactory.Create(id: 3, email: "user1@example.com");
		var member2 = UserFactory.Create(id: 4, email: "user2@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = admin,
			Members = { admin, payer, member1, member2 },
			Status = GroupStatus.Active
		};

		var createExpenseDto = new CreateExpenseDto(
			group.Id,
			payer.Id,
			"expense",
			"expense.png",
			"expense-description",
			new decimal(100),
			DateTime.Now,
			[
				new ExpenseParticipantDto(member1.Id, member1.Name, 50),
				new ExpenseParticipantDto(member2.Id, member2.Name, 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);

		var command = new CreateExpenseCommand(payer.Id, createExpenseDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(payer);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member1.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member1);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member2.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member2);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_expenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
		_expenseSplitRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ExpenseSplit>(), It.IsAny<CancellationToken>()), Times.Once);
		_expenseSplitRecordRepositoryMock.Verify(repo => repo.AddRangeAsync(It.IsAny<ICollection<ExpenseSplitRecord>>(), It.IsAny<CancellationToken>()), Times.Once);

	}

	[TestMethod]
	public async Task Handler_ShouldReturnInvalid_WhenParticipantDoesNotExist()
	{
		// Arrange
		var payer = UserFactory.Create();
		var admin = UserFactory.Create(id: 2, email: "admin@example.com");
		var member2 = UserFactory.Create(id: 3, email: "user2@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = admin,
			Members = { admin, payer, member2 },
			Status = GroupStatus.Active
		};

		var createExpenseDto = new CreateExpenseDto(
			group.Id,
			payer.Id,
			"expense",
			"expense.png",
			"expense-description",
			new decimal(100),
			DateTime.Now,
			[
				new ExpenseParticipantDto(13, "person", 50),
				new ExpenseParticipantDto(member2.Id, member2.Name, 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);

		var command = new CreateExpenseCommand(payer.Id, createExpenseDto);

		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(payer);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member2.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member2);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	[TestMethod]
	public async Task Handler_ShouldReturnInvalid_WhenPayingPersonDoesNotExist()
	{
		// Arrange
		var admin = UserFactory.Create(id: 1, email: "admin@example.com");
		var member1 = UserFactory.Create(id: 2, email: "user1@example.com");
		var member2 = UserFactory.Create(id: 3, email: "user2@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = admin,
			Members = { admin, member1, member2 },
			Status = GroupStatus.Active
		};

		var createExpenseDto = new CreateExpenseDto(
			group.Id,
			15,
			"expense",
			"expense.png",
			"expense-description",
			new decimal(100),
			DateTime.Now,
			[
				new ExpenseParticipantDto(13, member1.Name, 50),
				new ExpenseParticipantDto(member2.Id, member2.Name, 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);

		var command = new CreateExpenseCommand(15, createExpenseDto);

		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member1.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member1);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member2.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member2);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	
	[TestMethod]
	public async Task Handler_ShouldReturnInvalid_WhenPayerIdDoesNotMatchCommandUserId()
	{
		// Arrange
		var payer = UserFactory.Create();
		var admin = UserFactory.Create(id: 2, email: "admin@example.com");
		var member1 = UserFactory.Create(id: 3, email: "user1@example.com");
		var member2 = UserFactory.Create(id: 4, email: "user2@example.com");
	
		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = admin,
			Members = { admin, payer, member1, member2 },
			Status = GroupStatus.Active
		};
	
		var createExpenseDto = new CreateExpenseDto(
			group.Id,
			payer.Id,
			"expense",
			"expense.png",
			"expense-description",
			new decimal(100),
			DateTime.Now,
			[
				new ExpenseParticipantDto(member1.Id, member1.Name, 50),
				new ExpenseParticipantDto(member2.Id, member2.Name, 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);
	
		var command = new CreateExpenseCommand(member1.Id, createExpenseDto);
	
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(payer);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member1.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member1);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member2.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member2);
	
		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);
	
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());
	
		// Act
		var result = await _handler.Handle(command, CancellationToken.None);
	
		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Status.Should().Be(ResultStatus.Forbidden);
	}
	
}