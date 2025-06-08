using Ardalis.Result;
using MediatR;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record GetGroupJoinRequestsCommand(int GroupId)
  : IRequest<Result<GetGroupJoinRequestsDto>>;
