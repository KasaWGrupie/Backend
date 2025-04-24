using Moq;
using FluentValidation;
using FluentValidation.Results;
using Ardalis.Result;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using Ardalis.Specification;

namespace KasaWGrupie.Tests.Handlers.Users;

[TestClass]
public class GetUserByEmailHandlerTests
{
	private Mock<IRepositoryBase<User>> _userRepoMock = null!;
	private Mock<IValidator<GetUserByEmailCommand>> _validatorMock = null!;
	private GetUserByEmailHandler _handler = null!;

	private readonly string _validEmail = "test@example.com";

	[TestInitialize]
	public void Setup()
	{
		_userRepoMock = new Mock<IRepositoryBase<User>>();
		_validatorMock = new Mock<IValidator<GetUserByEmailCommand>>();
		_handler = new GetUserByEmailHandler(_userRepoMock.Object, _validatorMock.Object);
	}

	[TestMethod]
	public async Task Handle_ReturnsSuccess_WhenUserExists()
	{
		// Arrange
		var user = new User
		{
			Id = 1,
			Name = "Test User",
			Email = _validEmail,
			ProfilePictureUrl = "http://pic.url"
		};

		_validatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<GetUserByEmailCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		var command = new GetUserByEmailCommand(_validEmail);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(user.Email, result.Value.Email);
		Assert.AreEqual(user.Name, result.Value.Name);
	}

	[TestMethod]
	public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
	{
		// Arrange
		_validatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<GetUserByEmailCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		var command = new GetUserByEmailCommand("nonexistent@example.com");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.NotFound, result.Status);
	}

	[TestMethod]
	public async Task Handle_ReturnsInvalid_WhenValidationFails()
	{
		// Arrange
		var validationFailures = new List<ValidationFailure>
		{
			new ValidationFailure("Email", "Invalid email format.")
		};

		_validatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<GetUserByEmailCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		var command = new GetUserByEmailCommand("bademail");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Invalid, result.Status);
		Assert.AreEqual("Email", result.ValidationErrors.First().Identifier);
	}
}
