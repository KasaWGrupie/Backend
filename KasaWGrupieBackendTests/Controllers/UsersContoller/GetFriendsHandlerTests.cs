using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Users;
using Moq;

namespace KasaWGrupie.Tests.Controllers.UserControllers;

[TestClass]
public class GetFriendsHandlerTests
{
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IValidator<GetFriendsCommand>> _validatorMock;
	private GetFriendsHandler _handler;

	[TestInitialize]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_validatorMock = new Mock<IValidator<GetFriendsCommand>>();
		_handler = new GetFriendsHandler(_userRepositoryMock.Object, _validatorMock.Object);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
	{
		// Arrange
		var command = new GetFriendsCommand(1);
		var validationFailures = new List<FluentValidation.Results.ValidationFailure>
		{
			new("UserId", "UserId must be greater than 0.")
		};
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(ResultStatus.Invalid);
		result.ValidationErrors.Should().ContainSingle()
			.Which.ErrorMessage.Should().Be("UserId must be greater than 0.");
	}

	[TestMethod]
	public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
	{
		// Arrange
		var command = new GetFriendsCommand(1);
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetUserByIdWithFriendsSpecification>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(ResultStatus.NotFound);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnMappedFriends_WhenUserExists()
	{
		// Arrange
		var command = new GetFriendsCommand(1);
		var user = new User
		{
			Id = 1,
			Friends = new List<User>
			{
				new User { Id = 2, Name = "Friend 1", Email = "friend1@example.com", ProfilePictureUrl = "url1" },
				new User { Id = 3, Name = "Friend 2", Email = "friend2@example.com", ProfilePictureUrl = "url2" }
			},
			Name = "User",
			Email = "user@example.com"
		};

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetUserByIdWithFriendsSpecification>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(ResultStatus.Ok);
		result.Value.Should().HaveCount(2);

		var friend1 = result.Value.FirstOrDefault(f => f.Id == 2);
		friend1.Should().NotBeNull();
		friend1.Name.Should().Be("Friend 1");
		friend1.Email.Should().Be("friend1@example.com");
		friend1.ProfilePictureUrl.Should().Be("url1");

		var friend2 = result.Value.FirstOrDefault(f => f.Id == 3);
		friend2.Should().NotBeNull();
		friend2.Name.Should().Be("Friend 2");
		friend2.Email.Should().Be("friend2@example.com");
		friend2.ProfilePictureUrl.Should().Be("url2");
	}
}
