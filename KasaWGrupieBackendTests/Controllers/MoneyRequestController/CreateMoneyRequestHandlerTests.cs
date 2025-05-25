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
public class CreateMoneyRequestHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IRepositoryBase<PayRequest>> _payRequestRepositoryMock;
	private Mock<IValidator<CreateMoneyRequestDto>> _validatorMock;
	private CreateMoneyRequestHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_payRequestRepositoryMock = new Mock<IRepositoryBase<PayRequest>>();
		_validatorMock = new Mock<IValidator<CreateMoneyRequestDto>>();

		_handler = new CreateMoneyRequestHandler(
			_userRepositoryMock.Object,
			_groupRepositoryMock.Object,
			_payRequestRepositoryMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
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

		var dto = new CreateMoneyRequestDto(
			sender.Id,
			receiver.Id,
			new List<int> { group.Id }
		);

		var command = new CreateMoneyRequestCommand(dto);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyRequestDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepositoryMock.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(r => r.GetByIdAsync(receiver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiver);
		_groupRepositoryMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_payRequestRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<PayRequest>(), It.IsAny<CancellationToken>()), Times.Once);
		_payRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
	{
		// Arrange
		var dto = new CreateMoneyRequestDto(
			1,
			2,
			new List<int> { 1 }
			);
		var command = new CreateMoneyRequestCommand(dto);

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
			new List<int> { 1 }
			);
		var command = new CreateMoneyRequestCommand(dto);

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
			new List<int> { 1 }
			);
		var command = new CreateMoneyRequestCommand(dto);

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
			new List<int> { 1 }
			);
		var command = new CreateMoneyRequestCommand(dto);

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
			new List<int> { group.Id }
			);
		var command = new CreateMoneyRequestCommand(dto);

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
}