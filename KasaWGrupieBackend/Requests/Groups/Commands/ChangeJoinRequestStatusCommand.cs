using Ardalis.Result;
using MediatR;

namespace KasaWGrupie.API.Requests.Groups.Commands;

/// <summary>
/// Handler will change the status of a single join request.
/// </summary>
public sealed record ChangeJoinRequestStatusCommand(
  int GroupId,
  int RequestId,
  KasaWGrupie.Core.Enums.JoinRequestStatus Status
) : IRequest<Result>;
