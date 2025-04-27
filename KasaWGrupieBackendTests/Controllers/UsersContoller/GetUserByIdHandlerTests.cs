using Moq;
using FluentValidation;
using FluentValidation.Results;
using Ardalis.Result;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using Ardalis.Specification;

namespace KasaWGrupie.Tests.Controllers.UserControllers
{
	[TestClass]
	public class GetUserByIdHandlerTests
	{
		private Mock<IRepositoryBase<User>> _userRepositoryMock = null!;
		private Mock<IValidator<GetUserByIdCommand>> _validatorMock = null!;
		private GetUserByIdHandler _handler = null!;

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_validatorMock = new Mock<IValidator<GetUserByIdCommand>>();
			_handler = new GetUserByIdHandler(_userRepositoryMock.Object, _validatorMock.Object);
		}

		[TestMethod]
		public async Task Handle_UserExists_ReturnsUserDto()
		{
			// Arrange
			var command = new GetUserByIdCommand(1);
			var user = new User
			{
				Id = 1,
				Name = "John Doe",
				Email = "john.doe@example.com",
				ProfilePictureUrl = "https://example.com/john.jpg"
			};

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
				.ReturnsAsync(user);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.IsSuccess);
			Assert.IsNotNull(result.Value);
			Assert.AreEqual(user.Id, result.Value.Id);
			Assert.AreEqual(user.Name, result.Value.Name);
			Assert.AreEqual(user.Email, result.Value.Email);
			Assert.AreEqual(user.ProfilePictureUrl, result.Value.ProfilePictureUrl);
		}

		[TestMethod]
		public async Task Handle_UserDoesNotExist_ReturnsNotFound()
		{
			// Arrange
			var command = new GetUserByIdCommand(2);

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
				.ReturnsAsync((User?)null);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.AreEqual(ResultStatus.NotFound, result.Status);
		}

		[TestMethod]
		public async Task Handle_ValidationFails_ReturnsInvalid()
		{
			// Arrange
			var command = new GetUserByIdCommand(0);

			var validationFailure = new ValidationFailure("Id", "Id must be greater than 0");
			var validationResult = new ValidationResult(new[] { validationFailure });

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(validationResult);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.AreEqual(ResultStatus.Invalid, result.Status);
			Assert.IsTrue(result.ValidationErrors.Any(e => e.ErrorMessage == "Id must be greater than 0"));
		}
	}
}
