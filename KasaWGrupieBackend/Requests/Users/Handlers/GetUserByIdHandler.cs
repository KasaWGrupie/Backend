using MediatR;
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.Core.Entities;
using Ardalis.Specification;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdCommand, Result<GetUserDto>>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<GetUserByIdCommand> _validator;

	public GetUserByIdHandler(IRepositoryBase<User> userRepository, IValidator<GetUserByIdCommand> validator)
	{
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result<GetUserDto>> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

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