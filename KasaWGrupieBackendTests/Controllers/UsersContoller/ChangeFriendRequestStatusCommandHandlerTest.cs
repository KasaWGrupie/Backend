using Moq;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.DTOs.FriendRequest;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using KasaWGrupie.API.Requests.FriendRequests.sHandlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using Ardalis.Specification;

namespace KasaWGrupie.Tests.Controllers.UserControllers;

[TestClass]
public class ChangeFriendRequestStatusCommandHandlerTests
{
	private Mock<IRepositoryBase<FriendRequest>> _friendRequestRepoMock = null!;
	private Mock<IRepositoryBase<User>> _userRepoMock = null!;
	private Mock<IValidator<ChangeFriendRequestStatusCommand>> _validatorMock = null!;
	private ChangeFriendRequestStatusCommandHandler _handler = null!;

	private User _sender = null!;
	private User _receiver = null!;
	private FriendRequest _friendRequest = null!;

	[TestInitialize]
	public void Setup()
	{
		_friendRequestRepoMock = new Mock<IRepositoryBase<FriendRequest>>();
		_userRepoMock = new Mock<IRepositoryBase<User>>();
		_validatorMock = new Mock<IValidator<ChangeFriendRequestStatusCommand>>();

		_handler = new ChangeFriendRequestStatusCommandHandler(
			_friendRequestRepoMock.Object,
			_userRepoMock.Object,
			_validatorMock.Object
		);

		_sender = new User { Id = 1, Name = "Alice", Email = "a@a.com", ProfilePictureUrl = "" };
		_receiver = new User { Id = 2, Name = "Bob", Email = "b@b.com", ProfilePictureUrl = "" };

		_friendRequest = new FriendRequest
		{
			Id = 10,
			Sender = _sender,
			Receiver = _receiver,
			Status = FriendRequestStatus.Unconfirmed
		};
	}

	[TestMethod]
	public async Task ReturnsInvalid_WhenValidationFails()
	{
		// Arrange
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Confirmed" });
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Status", "Required") }));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.Status == ResultStatus.Invalid);
	}

	[TestMethod]
	public async Task ReturnsNotFound_WhenFriendRequestDoesNotExist()
	{
		// Arrange
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Confirmed" });
		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_friendRequestRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FriendRequest>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((FriendRequest?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.NotFound, result.Status);
	}

	[TestMethod]
	public async Task ReturnsInvalid_WhenConfirmingRejectedRequest()
	{
		// Arrange
		_friendRequest.Status = FriendRequestStatus.Rejected;
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Confirmed" });

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_friendRequestRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FriendRequest>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(_friendRequest);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Invalid, result.Status);
		Assert.AreEqual("Cannot confirm a rejected friend request.", result.ValidationErrors.First().ErrorMessage);
	}

	[TestMethod]
	public async Task ReturnsInvalid_WhenRejectingConfirmedRequest()
	{
		_friendRequest.Status = FriendRequestStatus.Confirmed;
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Rejected" });

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_friendRequestRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FriendRequest>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(_friendRequest);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.AreEqual(ResultStatus.Invalid, result.Status);
		Assert.AreEqual("Cannot reject a confirmed friend request.", result.ValidationErrors.First().ErrorMessage);
	}

	[TestMethod]
	public async Task UpdatesStatusAndAddsFriends_WhenConfirmed()
	{
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Confirmed" });

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_friendRequestRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FriendRequest>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(_friendRequest);

		_userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
		_userRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

		_friendRequestRepoMock.Setup(r => r.UpdateAsync(It.IsAny<FriendRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
		_friendRequestRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.IsTrue(_sender.Friends.Contains(_receiver));
		Assert.IsTrue(_receiver.Friends.Contains(_sender));
	}

	[TestMethod]
	public async Task UpdatesStatus_WhenRejected()
	{
		var command = new ChangeFriendRequestStatusCommand(10, new ChangeFriendRequestStatusDto { Status = "Rejected" });

		_validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_friendRequestRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FriendRequest>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(_friendRequest);

		_friendRequestRepoMock.Setup(r => r.UpdateAsync(It.IsAny<FriendRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
		_friendRequestRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(FriendRequestStatus.Rejected, _friendRequest.Status);
	}
}
