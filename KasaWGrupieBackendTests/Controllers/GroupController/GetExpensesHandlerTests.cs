using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Tests.Factories;
using Moq;

namespace KasaWGrupie.Tests;

[TestClass]
public class GetExpensesHandlerTests
{
    private Mock<IRepositoryBase<Expense>> _expenseRepositoryMock;
    private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
    private Mock<IValidator<GetExpensesCommand>> _validatorMock;
    private GetExpensesHandler _handler;
    
    [TestInitialize]
    public void Setup()
    {
        _expenseRepositoryMock = new Mock<IRepositoryBase<Expense>>();
        _groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
        _validatorMock = new Mock<IValidator<GetExpensesCommand>>();
        
        _handler = new GetExpensesHandler(
            _expenseRepositoryMock.Object,
            _groupRepositoryMock.Object,
            _validatorMock.Object
        );
    }

    [TestMethod]
    public async Task Handler_ShouldReturnSuccess()
    {
        // Arrange
        var admin = UserFactory.Create(email: "admin@example.com");
        var member1 = UserFactory.Create(email: "user1@example.com");
        var member2 = UserFactory.Create(email: "user2@example.com");
        
        var group = new Group
        {
            Id = 1,
            Name = "Name",
            Description = "Description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = admin,
            Members = new List<User> { admin },
            Status = GroupStatus.Active
        };
        
        var expense1 = new Expense
        {
            Id = 1,
            Amount = 100,
            Group = group,
            PayingPerson = admin,
            Name = "Expense 1",
            Description = "Expense 1 description",
            PictureUrl = "expense.png"
        };
        var expenseSplit1 = expense1.ExpenseSplit = new ExpenseSplit
        {
            Expense = expense1,
            Type = ExpenseSplitType.Equally
        };
        expenseSplit1.SplitRecords = new List<ExpenseSplitRecord> {
            new()
            {
                ExpenseSplit = expenseSplit1,
                Amount = 0,
                OwingPerson = member1 
            }
        };
        var expense2 = new Expense
        {
            Id = 2,
            Amount = 350,
            Group = group,
            PayingPerson = admin,
            Name = "Expense 2",
            Description = "Expense 2 description",
            PictureUrl = "expense2.png"
        };
        var expenseSplit2 = expense2.ExpenseSplit = new ExpenseSplit
        {
            Expense = expense2,
            Type = ExpenseSplitType.ByPercent
        };
        expenseSplit2.SplitRecords = new List<ExpenseSplitRecord> {
            new()
            {
                ExpenseSplit = expenseSplit2,
                Percentage = 25,
                OwingPerson = member1 
            },
            new()
            {
                ExpenseSplit = expenseSplit2,
                Percentage = 75,
                OwingPerson = member2 
            }
        };

        var command = new GetExpensesCommand(group.Id);

        _expenseRepositoryMock.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Expense>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Expense> { expense1, expense2 });
        
        _groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        
        _validatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<GetExpensesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        var dto1 = result.Value.First();
        dto1.PaidBy.Should().Be(admin.Id);
        dto1.ExpensePictureUri.Should().Be(expense1.PictureUrl);
        dto1.Amount.Should().Be(expense1.Amount);
        dto1.DivisionMethod.Should().Be(expense1.ExpenseSplit.Type.ToString());
        dto1.Participants.Should().HaveCount(1);
        var dto2 = result.Value.ElementAt(1);
        dto2.PaidBy.Should().Be(admin.Id);
        dto2.ExpensePictureUri.Should().Be(expense2.PictureUrl);
        dto2.Amount.Should().Be(expense2.Amount);
        dto2.DivisionMethod.Should().Be(expense2.ExpenseSplit.Type.ToString());
        dto2.Participants.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnNotFound_WhenGroupDoesNotExist()
    {
        // Arrange
        var admin = UserFactory.Create(email: "admin@example.com");
        var member1 = UserFactory.Create(email: "user1@example.com");
        var member2 = UserFactory.Create(email: "user2@example.com");
        
        var group = new Group
        {
            Id = 1,
            Name = "Name",
            Description = "Description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = admin,
            Members = new List<User> { admin },
            Status = GroupStatus.Active
        };
        
        var expense1 = new Expense
        {
            Id = 1,
            Amount = 100,
            Group = group,
            PayingPerson = admin,
            Name = "Expense 1",
            Description = "Expense 1 description",
            PictureUrl = "expense.png"
        };
        var expenseSplit1 = expense1.ExpenseSplit = new ExpenseSplit
        {
            Expense = expense1,
            Type = ExpenseSplitType.Equally
        };
        expenseSplit1.SplitRecords = new List<ExpenseSplitRecord> {
            new()
            {
                ExpenseSplit = expenseSplit1,
                Amount = 0,
                OwingPerson = member1 
            }
        };
        var expense2 = new Expense
        {
            Id = 2,
            Amount = 350,
            Group = group,
            PayingPerson = admin,
            Name = "Expense 2",
            Description = "Expense 2 description",
            PictureUrl = "expense2.png"
        };
        var expenseSplit2 = expense2.ExpenseSplit = new ExpenseSplit
        {
            Expense = expense2,
            Type = ExpenseSplitType.ByPercent
        };
        expenseSplit2.SplitRecords = new List<ExpenseSplitRecord> {
            new()
            {
                ExpenseSplit = expenseSplit2,
                Percentage = 25,
                OwingPerson = member1 
            },
            new()
            {
                ExpenseSplit = expenseSplit2,
                Percentage = 75,
                OwingPerson = member2 
            }
        };

        var command = new GetExpensesCommand(5);

        _expenseRepositoryMock.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Expense>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Expense> { expense1, expense2 });
        
        _groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        
        _validatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<GetExpensesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}