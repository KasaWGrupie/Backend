namespace KasaWGrupie.API.DTOs.Users;

public record GetUserDto(
    int Id,
    string Name,
    string Email,
    string? ProfilePictureUrl
    );