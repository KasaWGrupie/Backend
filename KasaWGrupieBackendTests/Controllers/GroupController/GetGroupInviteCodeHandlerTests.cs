using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation.Results;
using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using Moq;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Tests.Controllers.GroupController;

[TestClass]
public class GetGroupInviteCodeHandlerTests
{
	private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
	private Mock<IValidator<GetGroupInviteCodeCommand>> _validatorMock;
	private GetGroupInviteCodeHandler _handler;

	[TestInitialize]
	public void Setup()
	{
		_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
		_validatorMock = new Mock<IValidator<GetGroupInviteCodeCommand>>();
		_handler = new GetGroupInviteCodeHandler(_groupRepositoryMock.Object, _validatorMock.Object);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
	{
		// Arrange
		var command = new GetGroupInviteCodeCommand(0);
		var failures = new List<ValidationFailure> { new("GroupId", "GroupId must be greater than 0") };
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(failures));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Invalid, result.Status);
		Assert.AreEqual("GroupId", result.ValidationErrors.First().Identifier);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnNotFound_WhenGroupDoesNotExist()
	{
		// Arrange
		var command = new GetGroupInviteCodeCommand(1);
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_groupRepositoryMock.Setup(r => r.GetByIdAsync(command.GroupId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Group?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.NotFound, result.Status);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnExistingInviteCode_WhenGroupHasCode()
	{
		// Arrange
		var command = new GetGroupInviteCodeCommand(1);
		var group = new Group
		{
			Id = 1,
			InviteCode = "EXIST123",
			Admin = new User() { Name = String.Empty, Email = String.Empty, ProfilePictureUrl = String.Empty },
			Currency = new Currency() { Name = String.Empty },
			Description = String.Empty,
			Name = String.Empty,
			PictureUrl = String.Empty,
		};

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_groupRepositoryMock.Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Ok, result.Status);
		Assert.AreEqual("EXIST123", result.Value.Code);
		_groupRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task Handle_ShouldGenerateInviteCode_WhenMissing()
	{
		// Arrange
		var command = new GetGroupInviteCodeCommand(1);
		var group = new Group
		{
			Id = 1,
			InviteCode = null,
			Admin = new User() { Name = String.Empty, Email = String.Empty, ProfilePictureUrl = String.Empty },
			Currency = new Currency() { Name = String.Empty },
			Description = String.Empty,
			Name = String.Empty,
			PictureUrl = String.Empty
		};

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_groupRepositoryMock.Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Ok, result.Status);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.Code));
		Assert.AreEqual(12, result.Value.Code.Length);
		_groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
		_groupRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}