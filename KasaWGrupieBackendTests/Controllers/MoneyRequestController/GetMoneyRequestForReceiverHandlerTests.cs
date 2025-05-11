using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.API.Requests.MoneyRequest.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Tests.Factories;
using Moq;

namespace KasaWGrupieTests;

[TestClass]
public class GetMoneyRequestForReceiverHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IRepositoryBase<User>> _userRepositoryMock;
    private Mock<IRepositoryBase<PayRequest>> _payRequestRepositoryMock;
    private Mock<IValidator<GetMoneyRequestForReceiverCommand>> _validatorMock;
    private GetMoneyRequestForReceiverHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IRepositoryBase<User>>();
        _payRequestRepositoryMock = new Mock<IRepositoryBase<PayRequest>>();
        _validatorMock = new Mock<IValidator<GetMoneyRequestForReceiverCommand>>();

        _handler = new GetMoneyRequestForReceiverHandler(
            _userRepositoryMock.Object,
            _payRequestRepositoryMock.Object,
            _validatorMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WithEmptyList_WhenReceiverHasNoRequests()
    {
        // Arrange
        var receiver = UserFactory.Create();
        var command = new GetMoneyRequestForReceiverCommand(receiver.Id);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiver);

        _payRequestRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<PayRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayRequest>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WithRequestsList_WhenReceiverHasRequests()
    {
        // Arrange
        var receiver = UserFactory.Create();
        var sender = UserFactory.Create(id: 2, email: "sender@example.com");
        var group = new Group
        {
            Id = 1,
            Name = "Group 1",
            Description = "Group 1 description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = sender,
            Members = new List<User> { sender, receiver },
            Status = GroupStatus.Active       
        };

        var payRequests = new List<PayRequest>
        {
            new PayRequest
            {
                Id = 1,
                Sender = sender,
                SenderId = sender.Id,
                Receiver = receiver,
                ReceiverId = receiver.Id,
                Amount = 100m,
                GroupsToSettle = new List<Group> { group },
                PayRequestStatus = PayRequestStatus.Pending
            },
            new PayRequest
            {
                Id = 2,
                Sender = sender,
                SenderId = sender.Id,
                Receiver = receiver,
                ReceiverId = receiver.Id,
                Amount = 200m,
                GroupsToSettle = new List<Group> { group },
                PayRequestStatus = PayRequestStatus.Paid,
                EndDate = DateTime.Now
            }
        };

        var command = new GetMoneyRequestForReceiverCommand(receiver.Id);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiver);

        _payRequestRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<PayRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payRequests);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        
        var firstRequest = result.Value.First();
        firstRequest.Id.Should().Be(1);
        firstRequest.SenderId.Should().Be(sender.Id);
        firstRequest.RecipientId.Should().Be(receiver.Id);
        firstRequest.MoneyValue.Should().Be(100m);
        firstRequest.Groups.Should().ContainSingle(id => id == group.Id);
        firstRequest.Status.Should().Be(PayRequestStatus.Pending.ToString());
        firstRequest.EndDate.Should().BeNull();

        var secondRequest = result.Value.Last();
        secondRequest.Id.Should().Be(2);
        secondRequest.Status.Should().Be(PayRequestStatus.Paid.ToString());
        secondRequest.EndDate.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
    {
        // Arrange
        var command = new GetMoneyRequestForReceiverCommand(1);

        var validationFailure = new FluentValidation.Results.ValidationFailure("ReceiverId", "Invalid receiver id");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenReceiverDoesNotExist()
    {
        // Arrange
        var command = new GetMoneyRequestForReceiverCommand(1);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
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
        var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
        var group = new Group
        {
            Id = 1,
            Name = "Group 1",
            Description = "Group 1 description",
            PictureUrl = "pic.jpg",
            Currency = new Currency { Name = "USD" },
            Admin = sender,
            Members = new List<User> { sender, receiver },
            Status = GroupStatus.Active
        };

        List<PayRequest> payRequests =
        [
            new PayRequest
            {
                Id = 1,
                Sender = sender,
                SenderId = sender.Id,
                Receiver = receiver,
                ReceiverId = receiver.Id,
                Amount = 100m,
                GroupsToSettle = new List<Group> { group },
                PayRequestStatus = PayRequestStatus.Pending
            },
            new PayRequest
            {
                Id = 2,
                Sender = sender,
                SenderId = sender.Id,
                Receiver = receiver,
                ReceiverId = receiver.Id,
                Amount = 200m,
                GroupsToSettle = new List<Group> { group },
                PayRequestStatus = PayRequestStatus.Paid
            }
        ];

        var command = new GetMoneyRequestForReceiverCommand(receiver.Id, "Pending");

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiver);

        _payRequestRepositoryMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<PayRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payRequests.Where(r => r.PayRequestStatus == PayRequestStatus.Pending).ToList());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Status.Should().Be(PayRequestStatus.Pending.ToString());
    }

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenInvalidStatusProvided()
    {
        // Arrange
        var receiver = UserFactory.Create();
        var command = new GetMoneyRequestForReceiverCommand(receiver.Id, "InvalidStatus");

        var validationFailure = new FluentValidation.Results.ValidationFailure("Status", "Status must be a valid status value");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetMoneyRequestForReceiverCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }
}