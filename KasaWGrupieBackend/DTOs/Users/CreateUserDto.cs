namespace KasaWGrupie.API.DTOs.Users;

public record CreateUserDto(
    string Name,
    string Email,
    IFormFile? ProfilePicture
    );