using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.API.Requests.MoneyTransfers.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Tests.Factories;
using Moq;

namespace KasaWGrupie.Tests;

[TestClass]
public class GetMoneyTransferForRecipientHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IRepositoryBase<User>> _userRepositoryMock;
    private Mock<IRepositoryBase<MoneyTransfer>> _moneyTransferRepositoryMock;
    private Mock<IValidator<GetMoneyTransferForRecipientCommand>> _validatorMock;
    private GetMoneyTransferForRecipientHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IRepositoryBase<User>>();
        _moneyTransferRepositoryMock = new Mock<IRepositoryBase<MoneyTransfer>>();
        _validatorMock = new Mock<IValidator<GetMoneyTransferForRecipientCommand>>();

        _handler = new GetMoneyTransferForRecipientHandler(
            _userRepositoryMock.Object,
            _moneyTransferRepositoryMock.Object,
            _validatorMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WithEmptyList_WhenRecipientHasNoTransfers()
    {
        // Arrange
        var recipient = UserFactory.Create();
        var command = new GetMoneyTransferForRecipientCommand(recipient.Id);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        _moneyTransferRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<MoneyTransfer>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoneyTransfer>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WithTransfersList_WhenRecipientHasTransfers()
    {
        // Arrange
        var recipient = UserFactory.Create();
        var sender = UserFactory.Create(id: 2, email: "sender@example.com");
        var group = new Group
        {
            Id = 1,
            Name = "Group 1",
            Description = "Group 1 description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = sender,
            Members = new List<User> { sender, recipient },
            Status = GroupStatus.Active       
        };

        var moneyTransfers = new List<MoneyTransfer>
        {
            new MoneyTransfer
            {
                Id = 1,
                Sender = sender,
                SenderId = sender.Id,
                Recipient = recipient,
                RecipientId = recipient.Id,
                Amount = 100m,
                Group = group,
                GroupId = group.Id,
                Status = MoneyTransferStatus.Pending
            },
            new MoneyTransfer
            {
                Id = 2,
                Sender = sender,
                SenderId = sender.Id,
                Recipient = recipient,
                RecipientId = recipient.Id,
                Amount = 200m,
                Group = group,
                GroupId = group.Id,
                Status = MoneyTransferStatus.Confirmed,
                EndDate = DateTime.Now
            }
        };

        var command = new GetMoneyTransferForRecipientCommand(recipient.Id);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        _moneyTransferRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<MoneyTransfer>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(moneyTransfers);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        var firstTransfer = result.Value.First();
        firstTransfer.Id.Should().Be(1);
        firstTransfer.SenderId.Should().Be(sender.Id);
        firstTransfer.RecipientId.Should().Be(recipient.Id);
        firstTransfer.Amount.Should().Be(100m);
        firstTransfer.GroupId.Should().Be(group.Id);
        firstTransfer.Status.Should().Be(MoneyTransferStatus.Pending.ToString());
        firstTransfer.EndDate.Should().BeNull();

        var secondTransfer = result.Value.Last();
        secondTransfer.Id.Should().Be(2);
        secondTransfer.Status.Should().Be(MoneyTransferStatus.Confirmed.ToString());
        secondTransfer.EndDate.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
    {
        // Arrange
        var command = new GetMoneyTransferForRecipientCommand(1);

        var validationFailure = new FluentValidation.Results.ValidationFailure("RecipientId", "Invalid recipient id");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenRecipientDoesNotExist()
    {
        // Arrange
        var command = new GetMoneyTransferForRecipientCommand(1);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnFilteredResults_WhenStatusProvided()
    {
        // Arrange
        var sender = UserFactory.Create();
        var recipient = UserFactory.Create(id: 2, email: "recipient@example.com");
        var group = new Group
        {
            Id = 1,
            Name = "Group 1",
            Description = "Group 1 description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = sender,
            Members = new List<User> { sender, recipient },
            Status = GroupStatus.Active
        };

        List<MoneyTransfer> moneyTransfers =
        [
            new MoneyTransfer
            {
                Id = 1,
                Sender = sender,
                SenderId = sender.Id,
                Recipient = recipient,
                RecipientId = recipient.Id,
                Amount = 100m,
                Group = group,
                GroupId = group.Id,
                Status = MoneyTransferStatus.Pending
            },
            new MoneyTransfer
            {
                Id = 2,
                Sender = sender,
                SenderId = sender.Id,
                Recipient = recipient,
                RecipientId = recipient.Id,
                Amount = 200m,
                Group = group,
                GroupId = group.Id,
                Status = MoneyTransferStatus.Confirmed
            }
        ];

        var command = new GetMoneyTransferForRecipientCommand(recipient.Id, "Pending");

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        _moneyTransferRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<MoneyTransfer>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(moneyTransfers.Where(r => r.Status == MoneyTransferStatus.Pending).ToList());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Status.Should().Be(MoneyTransferStatus.Pending.ToString());
    }

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenInvalidStatusProvided()
    {
        // Arrange
        var recipient = UserFactory.Create();
        var command = new GetMoneyTransferForRecipientCommand(recipient.Id, "InvalidStatus");

        var validationFailure = new FluentValidation.Results.ValidationFailure("Status", "Status must be a valid status value");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyTransferForRecipientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }
}