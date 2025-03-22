using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.ImageService;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result>
{
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IImageService _imageService;
	private readonly IValidator<CreateUserDto> _validator;
	public CreateUserHandler(IRepositoryBase<User> userRepository, IImageService imageService, IValidator<CreateUserDto> validator)
	{
		_userRepository = userRepository;
		_imageService = imageService;
		_validator = validator;
	}
	public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request.CreateUserDto, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var imageUrl = string.Empty;
		if (request.CreateUserDto.ProfilePicture != null)
		{
			var uploadResult = await _imageService.UploadImageAsync(request.CreateUserDto.ProfilePicture, cancellationToken);
			if (uploadResult.IsSuccess)
			{
				imageUrl = uploadResult.Url;
			}
		}
		
		var user = new User
		{
			Name = request.CreateUserDto.Name,
			Email = request.CreateUserDto.Email,
			ProfilePictureUrl = imageUrl
		};
		
		await _userRepository.AddAsync(user, cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}