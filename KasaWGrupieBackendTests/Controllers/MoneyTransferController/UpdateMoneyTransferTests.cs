using Moq;
using KasaWGrupie.Core.Entities;
using FluentAssertions;
using KasaWGrupie.Tests.Factories;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.API.Requests.MoneyTransfers.Handlers;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Tests;

[TestClass]
public class UpdateMoneyTransferTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<MoneyTransfer>> _moneyTransferRepositoryMock;
	private Mock<IValidator<UpdateMoneyTransferDto>> _validatorMock;
	private UpdateMoneyTransferHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_moneyTransferRepositoryMock = new Mock<IRepositoryBase<MoneyTransfer>>();
		_validatorMock = new Mock<IValidator<UpdateMoneyTransferDto>>();

		_handler = new UpdateMoneyTransferHandler(
			_moneyTransferRepositoryMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnSuccess_WhenMoneyTransferStatusIsUpdatedSuccessfully()
	{
		// Arrange
		var sender = UserFactory.Create();
		var recipient = UserFactory.Create(email: "user2@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = sender,
			Members = { sender, recipient },
			Status = GroupStatus.Active
		};

		var moneyTransfer = new MoneyTransfer
		{
			Id = 1,
			Group = group,
			Amount = 5,
			Recipient = recipient,
			Sender = sender,
			Status = MoneyTransferStatus.Unconfirmed,
		};

		var updateMoneyTransferDto = new UpdateMoneyTransferDto(
			MoneyTransferStatus.Confirmed.ToString().ToLower()
		);
		
		var command = new UpdateMoneyTransferCommand(moneyTransfer.Id, updateMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(recipient);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_moneyTransferRepositoryMock.Setup(repo => repo.GetByIdAsync(moneyTransfer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(moneyTransfer);
		
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_moneyTransferRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<MoneyTransfer>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenMoneyTransferDoesNotExist()
	{
		// Arrange
		var updateMoneyTransferDto = new UpdateMoneyTransferDto(
			MoneyTransferStatus.Confirmed.ToString().ToLower()
		);
		
		var command = new UpdateMoneyTransferCommand(5, updateMoneyTransferDto);
		
		
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	
	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenStatusIsInvalid()
	{
		// Arrange
		var sender = UserFactory.Create();
		var recipient = UserFactory.Create(email: "user2@example.com");

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = sender,
			Members = { sender, recipient },
			Status = GroupStatus.Active
		};

		var moneyTransfer = new MoneyTransfer
		{
			Id = 1,
			Group = group,
			Amount = 5,
			Recipient = recipient,
			Sender = sender,
			Status = MoneyTransferStatus.Unconfirmed,
		};

		var updateMoneyTransferDto = new UpdateMoneyTransferDto(
			"fakeStatus"
		);
		
		var command = new UpdateMoneyTransferCommand(moneyTransfer.Id, updateMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(recipient);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_moneyTransferRepositoryMock.Setup(repo => repo.GetByIdAsync(moneyTransfer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(moneyTransfer);
		
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}

}