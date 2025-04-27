using MediatR;
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using Ardalis.Specification;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Persistence.Specifications.Users;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IValidator<DeleteUserCommand> _validator;

	public DeleteUserHandler(IRepositoryBase<User> userRepository, IValidator<DeleteUserCommand> validator)
	{
		_userRepository = userRepository;
		_validator = validator;
	}
	public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var userSpecification = new GetUserByIdWithActiveAndClosingGroupsSpecification(request.Id);

		var user = await _userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

		if (user == null)
		{
			return Result.NotFound();
		}

		if (user.Groups.Any())
		{
			return Result.Invalid(new List<ValidationError>
			{
				new ValidationError("User", "User cannot be deleted because they are part of active or closing groups.")
			});
		}

		await _userRepository.DeleteAsync(user, cancellationToken);

		return Result.Success();
	}
}