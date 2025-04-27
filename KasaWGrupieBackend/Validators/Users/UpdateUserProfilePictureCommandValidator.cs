using FluentValidation;
using global::KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class UpdateUserProfilePictureCommandValidator : AbstractValidator<UpdateUserProfilePictureCommand>
{
	public UpdateUserProfilePictureCommandValidator()
	{
		RuleFor(command => command.Id)
			.GreaterThan(0).WithMessage("User ID must be greater than 0.");

		RuleFor(command => command.UpdateUserProfilePictureDto)
			.NotNull().WithMessage("Profile picture data cannot be null.");

		RuleFor(command => command.UpdateUserProfilePictureDto.ProfilePicture)
			.Must(BeValidImage)
			.WithMessage("The file must be a valid image.");
	}

	private bool BeValidImage(IFormFile? file)
	{
		if (file == null) return true;

		var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
		var extension = Path.GetExtension(file.FileName);
		return allowedExtensions.Contains(extension?.ToLower());
	}
}
