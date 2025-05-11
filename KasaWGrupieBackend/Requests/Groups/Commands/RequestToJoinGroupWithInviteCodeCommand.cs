using KasaWGrupie.API.DTOs.Groups;
using MediatR;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record RequestToJoinGroupWithInviteCodeCommand(
	InviteCodeDto InviteCodeDto,
	string UserEmail
) : IRequest<Result>;