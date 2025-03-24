namespace KasaWGrupie.API.DTOs.Users;

public sealed record GetUserGroupsDto(
    int Id,
    string Name,
    string Currency,
    string AdminEmail
);

