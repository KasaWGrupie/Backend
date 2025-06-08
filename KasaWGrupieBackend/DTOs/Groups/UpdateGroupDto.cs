namespace KasaWGrupie.API.DTOs.Groups;

public sealed record UpdateGroupDto(
    string? Name,
    string? Description,
    string? Currency,
    string? AdminEmail,
    ICollection<string>? Members
    );