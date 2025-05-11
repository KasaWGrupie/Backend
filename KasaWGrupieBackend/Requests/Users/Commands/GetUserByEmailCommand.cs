using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.Users;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record GetUserByEmailCommand(
	string Email
) : IRequest<Result<GetUserDto>>;