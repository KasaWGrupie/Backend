using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Group id is required");

        RuleFor(x => x.Image)
            .Must(BeAValidImage)
            .WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png.")
            .When(x => x.Image != null);

        RuleFor(x => x.UpdateGroupDto)
            .SetValidator(new UpdateGroupDtoValidator());
    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }
}