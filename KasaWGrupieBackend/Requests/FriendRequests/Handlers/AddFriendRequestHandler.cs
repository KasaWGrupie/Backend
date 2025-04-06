using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.API.DTOs.FriendRequest;
using KasaWGrupie.Persistence.Specifications.Users;

namespace KasaWGrupie.API.Requests.FriendRequests.Handlers;

public class AddFriendRequestHandler : IRequestHandler<AddFriendRequestCommand, Result>
{
	private readonly IRepositoryBase<FriendRequest> _friendRequestRepository;
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<AddFriendRequestDto> _validator;

	public AddFriendRequestHandler(IRepositoryBase<FriendRequest> friendRequestRepository, IRepositoryBase<User> userRepository, IValidator<AddFriendRequestDto> validator)
	{
		_friendRequestRepository = friendRequestRepository;
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result> Handle(AddFriendRequestCommand request, CancellationToken cancellationToken)
	{
		var validationResult = _validator.Validate(request.AddFriendRequestDto);
		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var senderSpecification = new UserByIdSpecification(request.AddFriendRequestDto.SenderId);
		var receiverSpecification = new UserByIdSpecification(request.AddFriendRequestDto.ReceiverId);

		var sender = await _userRepository.FirstOrDefaultAsync(senderSpecification, cancellationToken);
		var receiver = await _userRepository.FirstOrDefaultAsync(receiverSpecification, cancellationToken);
		if (sender == null || receiver == null)
			return Result.NotFound();

		var friendRequest = new FriendRequest
		{
			Sender = sender,
			Receiver = receiver,
			Status = FriendRequestStatus.Unconfirmed
		};

		await _friendRequestRepository.AddAsync(friendRequest, cancellationToken);
		await _friendRequestRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
