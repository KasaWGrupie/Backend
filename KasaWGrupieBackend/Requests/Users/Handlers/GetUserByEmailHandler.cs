using MediatR;
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.Core.Entities;
using Ardalis.Specification;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Persistence.Specifications.Users;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailCommand, Result<GetUserDto>>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<GetUserByEmailCommand> _validator;

	public GetUserByEmailHandler(IRepositoryBase<User> userRepository, IValidator<GetUserByEmailCommand> validator)
	{
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result<GetUserDto>> Handle(GetUserByEmailCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var specification = new UserByEmailSpecification(request.Email);

		var user = await _userRepository.FirstOrDefaultAsync(specification, cancellationToken);

		if (user == null)
		{
			return Result<GetUserDto>.NotFound();
		}

		var userDto = new GetUserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			ProfilePictureUrl = user.ProfilePictureUrl
		};

		return userDto;
	}
}