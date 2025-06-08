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

		RuleFor(x => x.AdminId)
			.NotEmpty().WithMessage("Administrator id is required.")
			.GreaterThan(0);

		RuleFor(x => x.Members)
			.NotEmpty().WithMessage("At least one member is required.");

		RuleForEach(x => x.Members)
			.NotEmpty().WithMessage("Members id must be greater than 0.")
			.GreaterThan(0);

		RuleFor(x => x)
			.Must(x => x.Members != null && x.Members.Contains(x.AdminId))
			.WithMessage("The administrator must be a member of the group.");
	}

}
