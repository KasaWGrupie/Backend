using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.DTOs.Groups;

namespace KasaWGrupie.API.Controllers;

[Route("groups")]
[ApiController]
public sealed class GroupsController : ControllerBase
{
	private readonly IMediator _mediator;
	public GroupsController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Add new group
	/// </summary>
	/// <returns>Successfully inserted new group</returns>
	[TranslateResultToActionResult]
	[HttpPost]
	public async Task<Result> CreateGroup([FromForm] CreateGroupDto createGroupDto)
	{
		var command = new CreateGroupCommand(createGroupDto);
		var result = await _mediator.Send(command);

		return result;
	}

}
