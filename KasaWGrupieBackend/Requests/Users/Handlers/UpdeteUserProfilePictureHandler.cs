using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Infrastructure.ImageService;
using KasaWGrupie.Core.Entities;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class UpdateUserProfilePictureHandler : IRequestHandler<UpdateUserProfilePictureCommand, Result>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IImageService _imageService;
	private readonly IValidator<UpdateUserProfilePictureCommand> _validator;
	public UpdateUserProfilePictureHandler(IRepositoryBase<User> userRepository, IImageService imageService, IValidator<UpdateUserProfilePictureCommand> validator)
	{
		_userRepository = userRepository;
		_imageService = imageService;
		_validator = validator;
	}
	public async Task<Result> Handle(UpdateUserProfilePictureCommand request, CancellationToken cancellationToken)
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

		var imageUrl = string.Empty;

		if (request.ProfilePicture != null)
		{
			var uploadResult = await _imageService.UploadImageAsync(request.ProfilePicture, cancellationToken);
			if (uploadResult.IsSuccess)
			{
				imageUrl = uploadResult.Url;
			}
			else
			{
				return Result.Error("Image upload failed");
			}
		}

		user.ProfilePictureUrl = imageUrl;

		await _userRepository.UpdateAsync(user, cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}