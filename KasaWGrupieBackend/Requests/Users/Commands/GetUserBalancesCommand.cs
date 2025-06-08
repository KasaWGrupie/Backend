using Ardalis.Result;
using MediatR;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record GetUserBalancesCommand(int UserId)
  : IRequest<Result<GetUserBalancesDto>>;
