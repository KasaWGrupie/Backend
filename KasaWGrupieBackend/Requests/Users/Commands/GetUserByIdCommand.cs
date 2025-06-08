using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record GetUserByIdCommand(
	int Id) : IRequest<Result<GetUserDto>>;