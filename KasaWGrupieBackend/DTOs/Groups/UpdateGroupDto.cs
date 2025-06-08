namespace KasaWGrupie.API.DTOs.Groups;

public sealed record UpdateGroupDto(
    string? Name,
    string? Description,
    IFormFile? Image
    );