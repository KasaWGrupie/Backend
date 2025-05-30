using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.DTOs.Groups;

public sealed record JoinRequestDto(
  int Id,              // request identifier
  int RequesterId,     // user who asked to join
  string RequesterName, // their display name
  JoinRequestStatus Status         
);


public sealed record GetGroupJoinRequestsDto(
  int GroupId,
  IReadOnlyCollection<JoinRequestDto> JoinRequests
);