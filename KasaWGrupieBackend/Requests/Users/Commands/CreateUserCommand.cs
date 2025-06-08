using Ardalis.Result;
using KasaWGrupie.API.DTOs.Users;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Commands;

public record CreateUserCommand(
    CreateUserDto CreateUserDto,
    IFormFile? ProfilePicture
    ) : IRequest<Result>;