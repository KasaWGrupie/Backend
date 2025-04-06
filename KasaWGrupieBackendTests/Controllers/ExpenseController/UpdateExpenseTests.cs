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
public class UpdateExpenseTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<Expense>> _expenseRepositoryMock;
	private Mock<IRepositoryBase<ExpenseSplit>> _expenseSplitRepositoryMock;
	private Mock<IRepositoryBase<ExpenseSplitRecord>> _expenseSplitRecordRepositoryMock;
	private Mock<IValidator<UpdateExpenseDto>> _validatorMock;
	private UpdateExpenseHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_expenseRepositoryMock = new Mock<IRepositoryBase<Expense>>();
		_expenseSplitRepositoryMock = new Mock<IRepositoryBase<ExpenseSplit>>();
		_expenseSplitRecordRepositoryMock = new Mock<IRepositoryBase<ExpenseSplitRecord>>();
		_validatorMock = new Mock<IValidator<UpdateExpenseDto>>();

		_handler = new UpdateExpenseHandler(
			_expenseRepositoryMock.Object,
			_expenseSplitRepositoryMock.Object,
			_expenseSplitRecordRepositoryMock.Object,
			_userRepositoryMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnSuccess_WhenExpenseIsUpdatedSuccessfully()
	{
		// Arrange
		var payer = UserFactory.Create();
		var admin = UserFactory.Create(email: "admin@example.com");
		var member1 = UserFactory.Create(email: "user1@example.com");
		var member2 = UserFactory.Create(email: "user2@example.com");

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

		var expense = new Expense
		{
			Id = 1,
			Amount = 100,
			Group = group,
			PayingPerson = payer,
			PictureUrl = "old_expense.png"
		};
		var expenseSplit = expense.ExpenseSplit = new ExpenseSplit
		{
			Expense = expense,
			Type = ExpenseSplitType.Equally
		};
		expenseSplit.SplitRecords = new List<ExpenseSplitRecord> {
			new()
			{
				ExpenseSplit = expenseSplit,
				Amount = 0,
				OwingPerson = member1 
			}
		};
		
		var updateExpenseDto = new UpdateExpenseDto(
			expense.Id,
			null,
			"updated expense",
			"new_expense.png",
			"new-expense-description",
			new decimal(200),
			DateTime.Now,
			[
				new ExpenseParticipantDto(member1.Id, member1.Name, 50),
				new ExpenseParticipantDto(member2.Id, member2.Name, 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);

		var command = new UpdateExpenseCommand(updateExpenseDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(payer);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member1.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member1);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member2.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member2);

		_expenseRepositoryMock.Setup(repo => repo.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expense);
		
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_expenseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
		_expenseSplitRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<ExpenseSplit>(), It.IsAny<CancellationToken>()), Times.Once);
		_expenseSplitRecordRepositoryMock.Verify(repo => repo.AddRangeAsync(It.IsAny<ICollection<ExpenseSplitRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
		_expenseSplitRecordRepositoryMock.Verify(repo => repo.DeleteRangeAsync(It.IsAny<ICollection<ExpenseSplitRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
		expense.PayingPerson.Should().Be(payer);
		expense.PictureUrl.Should().Be("new_expense.png");
		expense.Amount.Should().Be(200);
		expenseSplit.Type.Should().Be(ExpenseSplitType.ByPercent);
		expenseSplit.SplitRecords.Should().HaveCount(2);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenParticipantDoesNotExist()
	{
		// Arrange
		var payer = UserFactory.Create();
		var admin = UserFactory.Create(email: "admin@example.com");
		var member1 = UserFactory.Create(email: "user1@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = admin,
			Members = { admin, payer, member1 },
			Status = GroupStatus.Active
		};

		var expense = new Expense
		{
			Id = 1,
			Amount = 100,
			Group = group,
			PayingPerson = payer,
			PictureUrl = "old_expense.png"
		};
		var expenseSplit = expense.ExpenseSplit = new ExpenseSplit
		{
			Expense = expense,
			Type = ExpenseSplitType.Equally
		};
		expenseSplit.SplitRecords = new List<ExpenseSplitRecord> {
			new()
			{
				ExpenseSplit = expenseSplit,
				Amount = 0,
				OwingPerson = member1 
			}
		};
		
		var updateExpenseDto = new UpdateExpenseDto(
			expense.Id,
			null,
			"updated expense",
			"new_expense.png",
			"new-expense-description",
			new decimal(200),
			DateTime.Now,
			[
				new ExpenseParticipantDto(member1.Id, member1.Name, 50),
				new ExpenseParticipantDto(123, "", 50)
			],
			ExpenseSplitType.ByPercent.ToString()
		);

		var command = new UpdateExpenseCommand(updateExpenseDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(payer);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(member1.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(member1);

		_expenseRepositoryMock.Setup(repo => repo.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expense);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateExpenseDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	
}