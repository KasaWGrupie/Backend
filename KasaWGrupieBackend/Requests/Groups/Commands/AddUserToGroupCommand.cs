using Ardalis.Result;
using MediatR;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record AddUserToGroupCommand(int GroupId, int MemberId)
  : IRequest<Result>;
