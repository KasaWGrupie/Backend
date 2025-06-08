using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using KasaWGrupie.Core.Entities;
using Moq;

[TestClass]
public class RequestToJoinGroupWithInviteCodeHandlerTests
{
	private Mock<IRepositoryBase<Group>> _groupRepoMock;
	private Mock<IRepositoryBase<User>> _userRepoMock;
	private Mock<IRepositoryBase<JoinRequest>> _joinRequestRepoMock;
	private Mock<IValidator<RequestToJoinGroupWithInviteCodeCommand>> _validatorMock;
	private RequestToJoinGroupWithInviteCodeHandler _handler;

	[TestInitialize]
	public void Setup()
	{
		_groupRepoMock = new Mock<IRepositoryBase<Group>>();
		_userRepoMock = new Mock<IRepositoryBase<User>>();
		_joinRequestRepoMock = new Mock<IRepositoryBase<JoinRequest>>();
		_validatorMock = new Mock<IValidator<RequestToJoinGroupWithInviteCodeCommand>>();
		_handler = new RequestToJoinGroupWithInviteCodeHandler(
			_groupRepoMock.Object,
			_userRepoMock.Object,
			_joinRequestRepoMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ValidRequest_ReturnsSuccess()
	{
		// Arrange
		var command = new RequestToJoinGroupWithInviteCodeCommand(
			new InviteCodeDto("ABC123"),
			"user@example.com"
		);

		var user = new User { Id = 1, Email = "user@example.com", ProfilePictureUrl = String.Empty, Name = String.Empty };
		var group = new Group
		{
			Id = 1,
			PictureUrl = String.Empty,
			Name = String.Empty,
			Description = String.Empty,
			Currency = new Currency() { Name = String.Empty },
			Admin = new User() { Id = 2, Email = String.Empty, ProfilePictureUrl = String.Empty, Name = String.Empty },
			JoinRequests = new List<JoinRequest>() // No existing requests
		};

		_validatorMock
			.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_groupRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		_joinRequestRepoMock
			.Setup(r => r.AddAsync(It.IsAny<JoinRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new JoinRequest() { Group = group, RequestingUser = user });

		_joinRequestRepoMock
			.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task Handle_InvalidRequest_ReturnsInvalidResult()
	{
		// Arrange
		var command = new RequestToJoinGroupWithInviteCodeCommand(
			new InviteCodeDto(""),
			""
		);

		var errors = new List<FluentValidation.Results.ValidationFailure>
	{
		new("UserEmail", "Invalid email 😵‍💫"),
		new("InviteCodeDto.Code", "Code is required 🧩")
	};

		_validatorMock
			.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.Status == Ardalis.Result.ResultStatus.Invalid);
		Assert.AreEqual(2, result.ValidationErrors.Count());
	}

	[TestMethod]
	public async Task Handle_UserDoesNotExist_ReturnsInvalid()
	{
		// Arrange
		var command = new RequestToJoinGroupWithInviteCodeCommand(
			new InviteCodeDto("ABC123"),
			"ghost@domain.com"
		);

		_validatorMock
			.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.Status == Ardalis.Result.ResultStatus.Invalid);
		Assert.AreEqual("UserEmail", result.ValidationErrors.First().Identifier);
	}

	[TestMethod]
	public async Task Handle_GroupDoesNotExist_ReturnsInvalid()
	{
		// Arrange
		var command = new RequestToJoinGroupWithInviteCodeCommand(
			new InviteCodeDto("XYZ789"),
			"user@example.com"
		);

		var user = new User { Id = 1, Email = "user@example.com", ProfilePictureUrl = String.Empty, Name = String.Empty };

		_validatorMock
			.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_groupRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Group?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.Status == Ardalis.Result.ResultStatus.Invalid);
		Assert.AreEqual("InviteCode", result.ValidationErrors.First().Identifier);
	}

	[TestMethod]
	public async Task Handle_JoinRequestAlreadyExists_ReturnsSuccess()
	{
		// Arrange
		var user = new User { Id = 1, Email = "user@example.com", ProfilePictureUrl = String.Empty, Name = String.Empty };
		var group = new Group
		{
			Id = 1,
			PictureUrl = String.Empty,
			Description = String.Empty,
			Name = String.Empty,
			Admin = new User() { Id = 2, Email = String.Empty, ProfilePictureUrl = String.Empty, Name = String.Empty },
			Currency = new Currency() { Name = String.Empty },
			JoinRequests = new List<JoinRequest>()
		};
		group.JoinRequests.Add(new JoinRequest { RequestingUserId = user.Id, Group = group, RequestingUser = user });

		var command = new RequestToJoinGroupWithInviteCodeCommand(
			new InviteCodeDto("CODE123"),
			"user@example.com"
		);

		_validatorMock
			.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult());

		_userRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_groupRepoMock
			.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(group);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
	}
}