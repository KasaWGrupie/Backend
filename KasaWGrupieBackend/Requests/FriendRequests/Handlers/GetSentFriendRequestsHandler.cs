using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using MediatR;
using KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.FriendRequest;
using FluentValidation;

namespace KasaWGrupie.API.Requests.FriendRequests.Handlers;

public class GetSentFriendRequestsHandler : IRequestHandler<GetSentFriendRequestsCommand, Result<ICollection<SentFriendRequestDisplayDto>>>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<GetSentFriendRequestsCommand> _validator;
	public GetSentFriendRequestsHandler(IRepositoryBase<User> userRepository, IValidator<GetSentFriendRequestsCommand> validator)
	{
		_validator = validator;
		_userRepository = userRepository;
	}
	public async Task<Result<ICollection<SentFriendRequestDisplayDto>>> Handle(GetSentFriendRequestsCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var specificaton = new GetUserByIdWithUnconfirmedFriendRequestsWithReceiverSpecification(request.UserId);

		var user = await _userRepository.FirstOrDefaultAsync(specificaton, cancellationToken);

		if (user == null)
		{
			return Result.NotFound("User with given id does not exist");
		}

		var friendRequests = user.SentFriendRequests
			.Select(r => new SentFriendRequestDisplayDto(
				r.Id,
				r.SenderId,
				r.ReceiverId,
				r.Receiver.Name,
				r.Receiver.ProfilePictureUrl
			))
			.ToList();

		return Result.Success<ICollection<SentFriendRequestDisplayDto>>(friendRequests);
	}
}
