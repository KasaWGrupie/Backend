using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.API.DTOs.Groups;

public sealed record JoinRequestDto(
    
  int groupId,     // group id
  int userId,     // user who asked to join
  JoinRequestStatus Status         
);


public sealed record GetGroupJoinRequestsDto(
  int GroupId,
  IReadOnlyCollection<JoinRequestDto> JoinRequests
);