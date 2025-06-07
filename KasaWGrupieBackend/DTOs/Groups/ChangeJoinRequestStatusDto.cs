using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.DTOs.Groups;

public sealed record ChangeJoinRequestStatusDto(
  JoinRequestStatus Status    // must be Confirmed or Rejected
);
