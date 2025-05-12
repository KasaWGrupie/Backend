using Ardalis.Result;
using KasaWGrupie.API.DTOs.Users;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Commands;

public record SearchUsersByPartialEmailCommand(
    string Email
    ) : IRequest<Result<List<GetUserDto>>>;