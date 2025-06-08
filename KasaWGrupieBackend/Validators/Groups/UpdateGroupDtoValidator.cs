using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Validators.Groups;

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Name)
                    .MaximumLength(ValidatorConstants.CreateGroupDtoConstants.NameMaxLength)
                    .WithMessage($"Group name cannot exceed {ValidatorConstants.CreateGroupDtoConstants.NameMaxLength} characters.")
                    .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(ValidatorConstants.CreateGroupDtoConstants.DescriptionMaxLength)
            .WithMessage($"Group description cannot exceed {ValidatorConstants.CreateGroupDtoConstants.DescriptionMaxLength} characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Currency)
            .Length(ValidatorConstants.CreateGroupDtoConstants.CurrencyMaxLength)
            .WithMessage($"Currency code must be exactly {ValidatorConstants.CreateGroupDtoConstants.CurrencyMaxLength} characters.")
            .When(x => x.Currency != null);

        RuleFor(x => x.AdminEmail)
            .MaximumLength(ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength)
            .WithMessage($"Administrator email cannot exceed {ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength} characters.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .When(x => x.AdminEmail != null);

        RuleFor(x => x.Members)
            .NotEmpty()
            .WithMessage("At least one member is required.")
            .When(x => x.Members != null);

        RuleForEach(x => x.Members)
            .EmailAddress().WithMessage("Each member must have a valid email address.")
            .MaximumLength(ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength)
            .WithMessage($"Member email cannot exceed {ValidatorConstants.CreateGroupDtoConstants.EmailMaxLength} characters.")
            .When(x => x.Members != null);

        RuleFor(x => x.Image)
            .Must(BeAValidImage)
            .WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png.")
            .When(x => x.Image != null);

        RuleFor(x => x)
            .Must(x => x.AdminEmail == null || (x.Members != null && x.Members.Contains(x.AdminEmail)))
            .WithMessage("The administrator must be a member of the group.")
            .When(x => x.AdminEmail != null);
    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }
}
