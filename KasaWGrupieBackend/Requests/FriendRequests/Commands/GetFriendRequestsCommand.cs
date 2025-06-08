using KasaWGrupie.API.DTOs.FriendRequest;
using MediatR;
using Ardalis.Result;


namespace KasaWGrupie.API.Requests.FriendRequests.Commands;

public sealed record GetFriendRequestsCommand(
	int UserId) : IRequest<Result<ICollection<RecievedFriendRequestDisplayDto>>>
{ }