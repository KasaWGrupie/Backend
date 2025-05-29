using Moq;
using FluentValidation;
using FluentValidation.Results;
using Ardalis.Result;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using Ardalis.Specification;

namespace KasaWGrupie.Tests.Controllers.UserControllers
{
	[TestClass]
	public class UpdateUserNameHandlerTests
	{
		private Mock<IRepositoryBase<User>> _userRepositoryMock;
		private Mock<IValidator<UpdateUserNameCommand>> _validatorMock;
		private UpdateUserNameHandler _handler;

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_validatorMock = new Mock<IValidator<UpdateUserNameCommand>>();
			_handler = new UpdateUserNameHandler(_userRepositoryMock.Object, _validatorMock.Object);
		}

		[TestMethod]
		public async Task Handle_ValidationFails_ReturnsInvalid()
		{
			// Arrange
			var command = new UpdateUserNameCommand(1, new UpdateUserNameDto("New Name"));

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required.") }));

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.AreEqual(ResultStatus.Invalid, result.Status);
		}

		[TestMethod]
		public async Task Handle_UserNotFound_ReturnsNotFound()
		{
			// Arrange
			var command = new UpdateUserNameCommand(1, new UpdateUserNameDto("New Name"));

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock
				.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync((User)null!);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.AreEqual(ResultStatus.NotFound, result.Status);
		}

		[TestMethod]
		public async Task Handle_ValidRequest_UpdatesUserNameAndReturnsSuccess()
		{
			// Arrange
			var user = new User
			{
				Id = 1,
				Name = "Old Name",
				Email = "test@example.com",
				ProfilePictureUrl = "profile.jpg"
			};

			var command = new UpdateUserNameCommand(1, new UpdateUserNameDto("New Name"));

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock
				.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(user);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.AreEqual(ResultStatus.Ok, result.Status);
			Assert.AreEqual("New Name", user.Name);
			_userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
