using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;

namespace KasaWGrupie.API.Validators.Groups;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.CreateGroupDto)
            .SetValidator(new CreateGroupDtoValidator());
        
        RuleFor(x => x.Image)
            .Must(BeAValidImage).When(x => x.Image != null)
            .WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png.");
    }
    
    
    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }
}