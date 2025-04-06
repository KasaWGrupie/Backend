using KasaWGrupie.API.DTOs.FriendRequest;
using MediatR;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.FriendRequests.Commands;

public sealed record AddFriendRequestCommand(
	AddFriendRequestDto AddFriendRequestDto) : IRequest<Result>;
