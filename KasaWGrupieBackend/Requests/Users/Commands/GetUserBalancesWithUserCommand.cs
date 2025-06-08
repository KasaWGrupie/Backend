// KasaWGrupie.API/Requests/Users/Commands/GetUserBalancesWithUserCommand.cs
using Ardalis.Result;
using MediatR;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record GetUserBalancesWithUserCommand(int UserId, int OtherUserId)
  : IRequest<Result<GetUserToUserBalancesDto>>;
