namespace KasaWGrupie.API.Requests.FriendRequests.Commands;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.FriendRequest;
using MediatR;

public sealed record ChangeFriendRequestStatusCommand(
	int UserId,
	int RequestId,
	ChangeFriendRequestStatusDto ChangeFriendRequestStatusDto) : IRequest<Result>;
