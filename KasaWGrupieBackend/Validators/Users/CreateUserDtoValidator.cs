using FluentValidation;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Validators.Users;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
	public CreateUserDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Group name is required.")
			.MaximumLength(ValidatorConstants.CreateUserDtoConstants.NameMaxLength)
			.WithMessage($"Group name cannot exceed {ValidatorConstants.CreateUserDtoConstants.NameMaxLength} characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.MaximumLength(ValidatorConstants.CreateUserDtoConstants.EmailMaxLength)
			.WithMessage($"Email cannot exceed {ValidatorConstants.CreateUserDtoConstants.EmailMaxLength} characters.")
			.EmailAddress().WithMessage("Invalid email format.");
		
		RuleFor(x => x.ProfilePicture)
			.Must(BeAValidImage).When(x => x.ProfilePicture != null)
			.WithMessage("Invalid picture format. Allowed formats: jpg, jpeg, png.");
		
	}

	private bool BeAValidImage(IFormFile? file)
	{
		if (file == null) return true;

		var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
		var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

		return allowedExtensions.Contains(fileExtension);
	}
}
