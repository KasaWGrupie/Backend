using MediatR;
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using Ardalis.Specification;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class UpdateUserNameHandler : IRequestHandler<UpdateUserNameCommand, Result>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<UpdateUserNameCommand> _validator;

	public UpdateUserNameHandler(IRepositoryBase<User> userRepository, IValidator<UpdateUserNameCommand> validator)
	{
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result> Handle(UpdateUserNameCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

		if (user == null)
		{
			return Result.NotFound();
		}

		user.Name = request.UpdateUserNameDto.Name;

		await _userRepository.UpdateAsync(user, cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}