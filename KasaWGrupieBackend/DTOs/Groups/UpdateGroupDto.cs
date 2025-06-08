namespace KasaWGrupie.API.DTOs.Groups;

public sealed record UpdateGroupDto(
    int GroupId,
    string? Name,
    string? Description,
    string? Currency,
    string? AdminEmail,
    ICollection<string>? Members
    );