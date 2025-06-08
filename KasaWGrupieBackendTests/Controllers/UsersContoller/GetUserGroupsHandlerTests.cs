using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Groups;
using KasaWGrupie.Persistence.Specifications.Users;
using Moq;

namespace KasaWGrupie.Tests.Controllers.UserControllers
{
	[TestClass]
	public class GetUserGroupsHandlerTests
	{
		private Mock<IRepositoryBase<Group>> _groupRepositoryMock = null!;
		private Mock<IRepositoryBase<User>> _userRepositoryMock = null!;
		private Mock<IValidator<GetUserGroupsCommand>> _validatorMock = null!;
		private GetUserGroupsHandler _handler = null!;

		[TestInitialize]
		public void Setup()
		{
			_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_validatorMock = new Mock<IValidator<GetUserGroupsCommand>>();

			_handler = new GetUserGroupsHandler(
				_groupRepositoryMock.Object,
				_userRepositoryMock.Object,
				_validatorMock.Object
			);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnSuccess_WhenUserExistsAndIsInGroups()
		{
			// Arrange
			var email = "user@example.com";
			var user = new User { Id = 1, Email = email, Name = "user1", ProfilePictureUrl = "image" };
			var groups = new List<Group>
			{
				new Group { Id = 1, Name = "Group A", PictureUrl = "group picture", Description = "Desc A", Currency = new Currency { Name = "PLN" }, Admin = user },
				new Group { Id = 2, Name = "Group B", PictureUrl = "group picture", Description = "Desc B", Currency = new Currency { Name = "USD" }, Admin = user }
			};

			var command = new GetUserGroupsCommand(email);

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(user);

			_groupRepositoryMock.Setup(r => r.ListAsync(It.Is<GroupsByMemberEmailSpecification>(s => s != null), It.IsAny<CancellationToken>()))
				.ReturnsAsync(groups);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeTrue();
			result.Value.Should().HaveCount(2);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var command = new GetUserGroupsCommand("ghost@example.com");

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult());

			_userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((User?)null);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.Status.Should().Be(ResultStatus.NotFound);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
		{
			// Arrange
			var command = new GetUserGroupsCommand("");
			var validationFailure = new ValidationFailure("UserEmail", "Email is required");

			_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult(new[] { validationFailure }));

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.Status.Should().Be(ResultStatus.Invalid);

		}
	}
}
