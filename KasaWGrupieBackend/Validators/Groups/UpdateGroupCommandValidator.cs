using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Group id is required");
        
        RuleFor(x => x.UpdateGroupDto)
            .SetValidator(new UpdateGroupDtoValidator());
    }
}