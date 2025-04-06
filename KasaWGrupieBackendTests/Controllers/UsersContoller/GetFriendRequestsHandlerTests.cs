using Moq;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.Requests.FriendRequests.Handlers;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Specification;

namespace KasaWGrupie.Tests.Controllers.UserControllers;

[TestClass]
public class GetFriendRequestsHandlerTests
{
	private Mock<IRepositoryBase<User>> _userRepoMock = null!;
	private Mock<IValidator<GetFriendRequestsCommand>> _validatorMock = null!;
	private GetFriendRequestsHandler _handler = null!;

	private User _user = null!;
	private User _receiver = null!;

	[TestInitialize]
	public void Setup()
	{
		_userRepoMock = new Mock<IRepositoryBase<User>>();
		_validatorMock = new Mock<IValidator<GetFriendRequestsCommand>>();

		_handler = new GetFriendRequestsHandler(_userRepoMock.Object, _validatorMock.Object);

		_receiver = new User
		{
			Id = 2,
			Name = "Bob",
			Email = "bob@example.com",
			ProfilePictureUrl = "url"
		};

		_user = new User
		{
			Id = 1,
			Name = "Alice",
			Email = "alice@example.com",
			ProfilePictureUrl = "pic",
			SentFriendRequests = new List<FriendRequest>
			{
				new FriendRequest
				{
					Id = 123,
					SenderId = 1,
					ReceiverId = 2,
					Sender = new User { Id = 1, Name = "Alice", ProfilePictureUrl = "pic", Email = "a@a.com" },
					Receiver = _receiver
				}
			}
		};
	}

	[TestMethod]
	public async Task ReturnsInvalid_WhenValidationFails()
	{
		// Arrange
		var command = new GetFriendRequestsCommand(1);
		var errors = new List<ValidationFailure> { new("UserId", "UserId is required") };

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(errors));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Invalid, result.Status);
		Assert.AreEqual("UserId", result.ValidationErrors.First().Identifier);
	}

	[TestMethod]
	public async Task ReturnsNotFound_WhenUserNotFound()
	{
		// Arrange
		var command = new GetFriendRequestsCommand(1);

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.NotFound, result.Status);
		Assert.AreEqual("User with given id does not exist", result.Errors.First());
	}

	[TestMethod]
	public async Task ReturnsSuccess_WithFriendRequests()
	{
		// Arrange
		var command = new GetFriendRequestsCommand(1);

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(_user);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Ok, result.Status);
		Assert.AreEqual(1, result.Value.Count);
		var dto = result.Value.First();
		Assert.AreEqual(123, dto.Id);
		Assert.AreEqual(1, dto.SenderId);
		Assert.AreEqual("Alice", dto.SenderName);
	}
}
