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
public class CreateMoneyTransferTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IRepositoryBase<MoneyTransfer>> _moneyTransferRepositoryMock;
	private Mock<IValidator<CreateMoneyTransferDto>> _validatorMock;
	private CreateMoneyTransferHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void Setup()
	{
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_moneyTransferRepositoryMock = new Mock<IRepositoryBase<MoneyTransfer>>();
		_validatorMock = new Mock<IValidator<CreateMoneyTransferDto>>();

		_handler = new CreateMoneyTransferHandler(
			_moneyTransferRepositoryMock.Object,
			_userRepositoryMock.Object,
			_groupRepositoryMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnSuccess_WhenMoneyTransferIsCreatedSuccessfully()
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

		var createMoneyTransferDto = new CreateMoneyTransferDto(
			sender.Id,
			recipient.Id,
			new decimal(5.0),
			group.Id
		);

		var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(recipient);

		_groupRepositoryMock.Setup(repo => 
				repo.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_moneyTransferRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MoneyTransfer>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenRecipientDoesNotExist()
	{
		// Arrange
		var sender = UserFactory.Create();

		var group = new Group
		{
			Id = 1,
			Name = "Group 1",
			Description = "Group 1 description",
			PictureUrl = "pic.jpg",
			Currency = new Currency { Name = "USD" },
			Admin = sender,
			Members = { sender },
			Status = GroupStatus.Active
		};

		var createMoneyTransferDto = new CreateMoneyTransferDto(
			sender.Id,
			7,
			new decimal(5.0),
			group.Id
		);

		var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);

		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	
	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenRecipientDoesNotBelongToGroup()
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
			Members = { sender }, // recipient is not a member
			Status = GroupStatus.Active
		};

		var createMoneyTransferDto = new CreateMoneyTransferDto(
			sender.Id,
			recipient.Id,
			new decimal(53.0),
			group.Id
		);

		var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(recipient.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(recipient);
		
		_groupRepositoryMock.Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
	
	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenGroupDoesNotExist()
	{
		// Arrange
		var sender = UserFactory.Create();
		var recipient = UserFactory.Create(email: "user2@example.com");

		var createMoneyTransferDto = new CreateMoneyTransferDto(
			sender.Id,
			recipient.Id,
			new decimal(13.0),
			1
		);

		var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
		
		
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateMoneyTransferDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}

}