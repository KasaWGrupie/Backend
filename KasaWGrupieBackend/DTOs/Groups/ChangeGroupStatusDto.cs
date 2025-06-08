namespace KasaWGrupie.API.DTOs.Groups;

public sealed record ChangeGroupStatusDto(
    int GroupId,                   // populated via “with” in controller
    Core.Enums.GroupStatus Status  // maps exactly to your enum
);
