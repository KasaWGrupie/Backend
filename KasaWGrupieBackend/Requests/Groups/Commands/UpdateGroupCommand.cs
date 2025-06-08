using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record UpdateGroupCommand(
    int UserId,
    int GroupId,
    UpdateGroupDto UpdateGroupDto,
    IFormFile? Image
    ) : IRequest<Result>;