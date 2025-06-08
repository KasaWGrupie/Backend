using KasaWGrupie.API.DTOs.FriendRequest;
using MediatR;
using Ardalis.Result;


namespace KasaWGrupie.API.Requests.FriendRequests.Commands;

public sealed record GetSentFriendRequestsCommand(
	int UserId) : IRequest<Result<ICollection<SentFriendRequestDisplayDto>>>
{ }