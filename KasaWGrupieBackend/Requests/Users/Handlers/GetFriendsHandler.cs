using MediatR;
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.Core.Entities;
using Ardalis.Specification;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Persistence.Specifications.Users;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class GetFriendsHandler : IRequestHandler<GetFriendsCommand, Result<ICollection<GetUserDto>>>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<GetFriendsCommand> _validator;

	public GetFriendsHandler(IRepositoryBase<User> userRepository, IValidator<GetFriendsCommand> validator)
	{
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result<ICollection<GetUserDto>>> Handle(GetFriendsCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var specification = new GetUserByIdWithFriendsSpecification(request.UserId);

		var user = await _userRepository.FirstOrDefaultAsync(specification, cancellationToken);

		if (user == null)
		{
			return Result<ICollection<GetUserDto>>.NotFound();
		}

		var friendsDtos = user.Friends
		.Select(friend => new GetUserDto
		{
			Id = friend.Id,
			Name = friend.Name,
			Email = friend.Email,
			ProfilePictureUrl = friend.ProfilePictureUrl
		})
		.ToList();

		return friendsDtos;
	}
}