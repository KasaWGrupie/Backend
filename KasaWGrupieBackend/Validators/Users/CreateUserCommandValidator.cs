using FluentValidation;
using KasaWGrupie.API.Requests.Users.Commands;

namespace KasaWGrupie.API.Validators.Users;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .Must(BeAValidImage).When(x => x.ProfilePicture != null)
            .WithMessage("Invalid picture format. Allowed formats: jpg, jpeg, png.");

        RuleFor(x => x.CreateUserDto)
            .SetValidator(new CreateUserDtoValidator());
    }
	
    private static bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }
}