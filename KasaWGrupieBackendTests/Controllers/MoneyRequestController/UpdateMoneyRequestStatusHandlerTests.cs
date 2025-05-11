using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.API.Requests.MoneyRequest.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Tests.Factories;
using Moq;

namespace KasaWGrupieTests;

[TestClass]
public class UpdateMoneyRequestStatusHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IRepositoryBase<PayRequest>> _payRequestRepositoryMock;
    private Mock<IValidator<UpdateMoneyRequestStatusDto>> _validatorMock;
    private UpdateMoneyRequestStatusHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [TestInitialize]
    public void Setup()
    {
        _payRequestRepositoryMock = new Mock<IRepositoryBase<PayRequest>>();
        _validatorMock = new Mock<IValidator<UpdateMoneyRequestStatusDto>>();

        _handler = new UpdateMoneyRequestStatusHandler(
            _payRequestRepositoryMock.Object,
            _validatorMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WhenUpdateIsValid()
    {
        // Arrange
        var sender = UserFactory.Create();
        var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
        
        var dto = new UpdateMoneyRequestStatusDto("Paid");
        var command = new UpdateMoneyRequestStatusCommand(1, dto);
        var payRequest = new PayRequest
        {
            Id = 1,
            Sender = sender,
            Receiver = receiver,
            PayRequestStatus = PayRequestStatus.Pending
        };

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyRequestStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _payRequestRepositoryMock.Setup(r => r.GetByIdAsync(command.RequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payRequest.PayRequestStatus.Should().Be(PayRequestStatus.Paid);
        _payRequestRepositoryMock.Verify(repo => repo.UpdateAsync(payRequest, It.IsAny<CancellationToken>()), Times.Once);
        _payRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldSetEndDate_WhenStatusChangedToPaid()
    {
        // Arrange
        var sender = UserFactory.Create();
        var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
        
        var dto = new UpdateMoneyRequestStatusDto("Paid");
        var command = new UpdateMoneyRequestStatusCommand(1, dto);
        var payRequest = new PayRequest
        {
            Id = 1,
            Sender = sender,
            Receiver = receiver,
            PayRequestStatus = PayRequestStatus.Paid
        };

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyRequestStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _payRequestRepositoryMock.Setup(r => r.GetByIdAsync(command.RequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payRequest.PayRequestStatus.Should().Be(PayRequestStatus.Paid);
        payRequest.EndDate.Should().NotBeNull();
        payRequest.EndDate.Value.Date.Should().Be(DateTime.Now.Date);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
    {
        // Arrange
        var dto = new UpdateMoneyRequestStatusDto("Invalid");
        var command = new UpdateMoneyRequestStatusCommand(1, dto);

        var validationFailure = new FluentValidation.Results.ValidationFailure("Status", "Invalid status");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyRequestStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        _payRequestRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<PayRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenPayRequestDoesNotExist()
    {
        // Arrange
        var dto = new UpdateMoneyRequestStatusDto("Paid");
        var command = new UpdateMoneyRequestStatusCommand(1, dto);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyRequestStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _payRequestRepositoryMock.Setup(r => r.GetByIdAsync(command.RequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayRequest?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }


    // [TestMethod]
    // public async Task Handle_ShouldReturnInvalid_WhenTryingToUpdateClosedRequest()
    // {
    //     // Arrange
    //     var sender = UserFactory.Create();
    //     var receiver = UserFactory.Create(id: 2, email: "receiver@example.com");
    //     
    //     var dto = new UpdateMoneyRequestStatusDto(1, "Paid");
    //     var command = new UpdateMoneyRequestStatusCommand(dto);
    //     var payRequest = new PayRequest
    //     {
    //         Id = 1,
    //         Sender = sender,
    //         Receiver = receiver,
    //         PayRequestStatus = PayRequestStatus.Closed
    //     };
    //
    //     _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyRequestStatusDto>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    //
    //     _payRequestRepositoryMock.Setup(r => r.GetByIdAsync(command.RequestId, It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(payRequest);
    //
    //     // Act
    //     var result = await _handler.Handle(command, CancellationToken.None);
    //
    //     // Assert
    //     result.IsSuccess.Should().BeFalse();
    //     result.Status.Should().Be(ResultStatus.Invalid);
    // }
}