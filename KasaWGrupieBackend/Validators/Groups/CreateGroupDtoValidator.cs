using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Validators.Groups;

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
	public CreateGroupDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Group name is required.")
			.MaximumLength(ValidatorConstants.CreateGroupDtoConstants.NameMaxLength)
			.WithMessage($"Group name cannot exceed {ValidatorConstants.CreateGroupDtoConstants.NameMaxLength} characters.");

		RuleFor(x => x.Description)
			.MaximumLength(ValidatorConstants.CreateGroupDtoConstants.DescriptionMaxLength)
			.WithMessage($"Group description cannot exceed {ValidatorConstants.CreateGroupDtoConstants.DescriptionMaxLength} characters.");

		RuleFor(x => x.Currency)
			.NotEmpty().WithMessage("Currency is required.")
			.Length(ValidatorConstants.CreateGroupDtoConstants.CurrencyMaxLength)
			.WithMessage($"Currency code must be exactly {ValidatorConstants.CreateGroupDtoConstants.CurrencyMaxLength} characters.");

		RuleFor(x => x.AdminEmail)
			.NotEmpty().WithMessage("Administrator email is required.")
			.MaximumLength(ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength)
			.WithMessage($"Administrator email cannot exceed {ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength} characters.")
			.EmailAddress().WithMessage("Invalid email format.");

		RuleFor(x => x.Members)
			.NotEmpty().WithMessage("At least one member is required.");

		RuleForEach(x => x.Members)
			.EmailAddress().WithMessage("Each member must have a valid email address.")
			.MaximumLength(ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength)
			.WithMessage($"Member email cannot exceed {ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength} characters.");

		RuleFor(x => x.Image)
			.Must(BeAValidImage).When(x => x.Image != null)
			.WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png.");

		RuleFor(x => x)
			.Must(x => x.Members != null && x.Members.Contains(x.AdminEmail))
			.WithMessage("The administrator must be a member of the group.");
	}

	private bool BeAValidImage(IFormFile? file)
	{
		if (file == null) return true;

		var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
		var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

		return allowedExtensions.Contains(fileExtension);
	}
}
