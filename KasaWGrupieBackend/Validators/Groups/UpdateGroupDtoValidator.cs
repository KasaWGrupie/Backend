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

        
    }

}