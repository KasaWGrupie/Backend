using KasaWGrupie.API.DTOs.Users;
using MediatR;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record GetFriendsCommand(
	int UserId) : IRequest<Result<ICollection<GetUserDto>>>;
