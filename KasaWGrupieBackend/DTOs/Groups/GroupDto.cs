namespace KasaWGrupie.API.DTOs.Groups;

public sealed record GroupDto(
    int Id,
    string Name,
    string? Description,
    string? PictureUrl,
    string Currency,
    int AdminId,
    string Status,
    IReadOnlyCollection<int> Members
);
