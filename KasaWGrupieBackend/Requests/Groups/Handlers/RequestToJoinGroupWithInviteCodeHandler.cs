using MediatR;
using KasaWGrupie.API.Requests.Groups.Commands;
using Ardalis.Result;
using FluentValidation;
using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Groups;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class RequestToJoinGroupWithInviteCodeHandler : IRequestHandler<RequestToJoinGroupWithInviteCodeCommand, Result>
{
	private readonly IRepositoryBase<Group> _groupRepository;
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IRepositoryBase<JoinRequest> _joinRequestRepository;
	private readonly IValidator<RequestToJoinGroupWithInviteCodeCommand> _validator;

	public RequestToJoinGroupWithInviteCodeHandler(
		IRepositoryBase<Group> groupRepository,
		IRepositoryBase<User> userRepository,
		IRepositoryBase<JoinRequest> joinRequestRepository,
		IValidator<RequestToJoinGroupWithInviteCodeCommand> validator)
	{
		_groupRepository = groupRepository;
		_userRepository = userRepository;
		_joinRequestRepository = joinRequestRepository;
		_validator = validator;
	}
	public async Task<Result> Handle(RequestToJoinGroupWithInviteCodeCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var userSpecification = new UserByEmailSpecification(request.UserEmail);

		var user = await _userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

		if (user == null)
		{

			return Result.Invalid(new ValidationError("UserEmail", "User does not exist."));
		}

		var groupSpecification = new GroupByInviteCodeWithJoinRequestsSpecification(request.InviteCodeDto.Code);

		var group = await _groupRepository.FirstOrDefaultAsync(groupSpecification, cancellationToken);

		if (group == null)
		{
			return Result.Invalid(new ValidationError("InviteCode", "Group with this invite code does not exist."));
		}

		if (group.JoinRequests.Any(x => x.RequestingUserId == user.Id))
		{
			return Result.Success();
		}

		var joinRequest = new JoinRequest
		{
			RequestingUserId = user.Id,
			GroupId = group.Id,
			Status = JoinRequestStatus.Unconfirmed,
			RequestingUser = user,
			Group = group
		};

		await _joinRequestRepository.AddAsync(joinRequest, cancellationToken);
		await _joinRequestRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
