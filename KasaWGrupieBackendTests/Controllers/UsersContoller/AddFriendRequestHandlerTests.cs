using Moq;
using FluentValidation;
using FluentValidation.Results;
using Ardalis.Result;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.DTOs.FriendRequest;
using KasaWGrupie.API.Requests.FriendRequests.Handlers;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Specification;
using KasaWGrupie.Tests.Factories;


namespace KasaWGrupie.Tests.Controllers.UserControllers;

[TestClass]
public class AddFriendRequestHandlerTests
{
	private Mock<IRepositoryBase<FriendRequest>> _friendRequestRepoMock = null!;
	private Mock<IRepositoryBase<User>> _userRepoMock = null!;
	private Mock<IValidator<AddFriendRequestDto>> _validatorMock = null!;
	private AddFriendRequestHandler _handler = null!;

	[TestInitialize]
	public void Setup()
	{
		_friendRequestRepoMock = new Mock<IRepositoryBase<FriendRequest>>();
		_userRepoMock = new Mock<IRepositoryBase<User>>();
		_validatorMock = new Mock<IValidator<AddFriendRequestDto>>();

		_handler = new AddFriendRequestHandler(
			_friendRequestRepoMock.Object,
			_userRepoMock.Object,
			_validatorMock.Object);
	}

	[TestMethod]
	public async Task Handle_ReturnsSuccess_WhenValidRequest()
	{
		// Arrange
		var dto = new AddFriendRequestDto { SenderId = 1, ReceiverId = 2 };
		var command = new AddFriendRequestCommand(dto);

		var sender = UserFactory.Create(1, "sender@example.com");
		var receiver = UserFactory.Create(2, "receiver@example.com");

		_validatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock
			.SetupSequence(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(sender)
			.ReturnsAsync(receiver);

		// Act
		var result = await _handler.Handle(command, default);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		_friendRequestRepoMock.Verify(r => r.AddAsync(It.IsAny<FriendRequest>(), It.IsAny<CancellationToken>()), Times.Once);
		_friendRequestRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task Handle_ReturnsInvalid_WhenValidationFails()
	{
		// Arrange
		var dto = new AddFriendRequestDto { SenderId = 0, ReceiverId = 0 };
		var command = new AddFriendRequestCommand(dto);

		var failures = new List<ValidationFailure>
		{
			new("SenderId", "Sender is required"),
			new("ReceiverId", "Receiver is required")
		};

		_validatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(failures));

		// Act
		var result = await _handler.Handle(command, default);

		// Assert
		Assert.IsTrue(result.Status == ResultStatus.Invalid);
		Assert.AreEqual(2, result.ValidationErrors.Count());
	}

	[TestMethod]
	public async Task Handle_ReturnsNotFound_WhenSenderOrReceiverMissing()
	{
		// Arrange
		var dto = new AddFriendRequestDto { SenderId = 1, ReceiverId = 2 };
		var command = new AddFriendRequestCommand(dto);

		_validatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock
			.SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null) // sender not found
			.ReturnsAsync(UserFactory.Create(2));

		// Act
		var result = await _handler.Handle(command, default);

		// Assert
		Assert.AreEqual(ResultStatus.NotFound, result.Status);
	}
}
