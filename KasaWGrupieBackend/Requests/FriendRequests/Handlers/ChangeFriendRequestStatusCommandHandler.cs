using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using FluentValidation;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.FriendRequests;


namespace KasaWGrupie.API.Requests.FriendRequests.Handlers;

public class ChangeFriendRequestStatusCommandHandler : IRequestHandler<ChangeFriendRequestStatusCommand, Result>
{
	private readonly IRepositoryBase<FriendRequest> _friendRequestRepository;
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<ChangeFriendRequestStatusCommand> _validator;
	public ChangeFriendRequestStatusCommandHandler(IRepositoryBase<FriendRequest> friendRequestRepository, IRepositoryBase<User> userRepository, IValidator<ChangeFriendRequestStatusCommand> validator)
	{
		_friendRequestRepository = friendRequestRepository;
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result> Handle(ChangeFriendRequestStatusCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var friendRequestSpecification = new GetFriendRequestByIdWithUsersSpecification(request.RequestId);

		var friendRequest = await _friendRequestRepository.FirstOrDefaultAsync(friendRequestSpecification, cancellationToken);

		if (friendRequest == null)
		{
			return Result.NotFound();
		}

		if (request.UserId != friendRequest.ReceiverId)
		{
			return Result.Forbidden("Only request receiver can change friend request status.");
		}

		var status = Enum.Parse<FriendRequestStatus>(request.ChangeFriendRequestStatusDto.Status);

		if (status == FriendRequestStatus.Confirmed && friendRequest.Status == FriendRequestStatus.Rejected)
		{
			return Result.Invalid(new ValidationError("Status", "Cannot confirm a rejected friend request."));
		}

		if (status == FriendRequestStatus.Rejected && friendRequest.Status == FriendRequestStatus.Confirmed)
		{
			return Result.Invalid(new ValidationError("Status", "Cannot reject a confirmed friend request."));
		}

		if (status == FriendRequestStatus.Confirmed && friendRequest.Status == FriendRequestStatus.Unconfirmed)
		{
			friendRequest.Sender.Friends.Add(friendRequest.Receiver);
			friendRequest.Receiver.Friends.Add(friendRequest.Sender);

			await _userRepository.UpdateAsync(friendRequest.Sender, cancellationToken);
			await _userRepository.UpdateAsync(friendRequest.Receiver, cancellationToken);
			await _userRepository.SaveChangesAsync(cancellationToken);
		}

		friendRequest.Status = status;

		await _friendRequestRepository.UpdateAsync(friendRequest, cancellationToken);
		await _friendRequestRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();

	}
}
