using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record CreateGroupCommand(
	CreateGroupDto CreateGroupDto,
	IFormFile? Image
	) : IRequest<Result>;