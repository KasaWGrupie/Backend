using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Ardalis.Result;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.API.Requests.Users.Commands;
using Ardalis.Specification;
using System.Collections.Generic;
using System.Linq;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Tests.Handlers.Users
{
	[TestClass]
	public class DeleteUserHandlerTests
	{
		private Mock<IRepositoryBase<User>> _userRepositoryMock = null!;
		private Mock<IValidator<DeleteUserCommand>> _validatorMock = null!;
		private DeleteUserHandler _handler = null!;

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_validatorMock = new Mock<IValidator<DeleteUserCommand>>();
			_handler = new DeleteUserHandler(_userRepositoryMock.Object, _validatorMock.Object);
		}

		[TestMethod]
		public async Task Handle_UserExistsAndNoActiveGroups_DeletesUser_ReturnsSuccess()
		{
			// Arrange
			var command = new DeleteUserCommand(1);
			var user = new User { Id = 1, Groups = new List<Group>(), Email = "user@test.com", Name = "user" };

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock
				.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(user);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.IsSuccess);
			_userRepositoryMock.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Handle_UserExistsButHasActiveGroups_ReturnsInvalid()
		{
			// Arrange
			var command = new DeleteUserCommand(2);
			var user = new User
			{
				Id = 2,
				Groups = new List<Group>
				{
					new Group
					{
						Name = "Test Group",
						PictureUrl = "http://example.com/pic.jpg",
						Description = "Test description",
						CurrencyId = 1,
						Currency = new Currency { Id = 1, Name = "PLN" }, // Musisz mieć też odpowiedni Currency
					    AdminId = 1,
						Admin = new User { Id = 1, Name = "Admin", Email = "admin@test.com", ProfilePictureUrl = "http://example.com/admin.jpg" },
						Status = GroupStatus.Active
					}
				},
				Email = "user@test.com",
				Name = "user"
			};

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock
				.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(user);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Status == ResultStatus.Invalid);
			Assert.AreEqual("User", result.ValidationErrors.First().Identifier);
		}

		[TestMethod]
		public async Task Handle_UserNotFound_ReturnsNotFound()
		{
			// Arrange
			var command = new DeleteUserCommand(3);

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock
				.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((User?)null);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Status == ResultStatus.NotFound);
		}

		[TestMethod]
		public async Task Handle_ValidationFails_ReturnsInvalid()
		{
			// Arrange
			var command = new DeleteUserCommand(4);

			var validationFailures = new List<ValidationFailure>
			{
				new ValidationFailure("Id", "Id must be greater than 0")
			};

			_validatorMock
				.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult(validationFailures));

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Status == ResultStatus.Invalid);
			Assert.AreEqual("Id", result.ValidationErrors.First().Identifier);
		}
	}
}
