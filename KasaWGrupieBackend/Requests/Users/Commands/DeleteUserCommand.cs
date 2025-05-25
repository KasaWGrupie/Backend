using MediatR;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.Users.Commands;

public sealed record DeleteUserCommand(
	int Id) : IRequest<Result>;