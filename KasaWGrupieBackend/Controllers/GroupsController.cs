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

	[TranslateResultToActionResult]
	[HttpGet("{groupId:int}/expenses")]
	public async Task<Result<ICollection<GetExpensesDto>>> GetExpenses([FromRoute] int groupId)
	{
		var command = new GetExpensesCommand(groupId);
		var result = await _mediator.Send(command);
		
		return result;
	}

    




    [TranslateResultToActionResult]
    [HttpPut("{groupId}")]
    public async Task<Result> UpdateGroup(int groupId, [FromForm] UpdateGroupDto updateGroupDto)
    {
        var dto = updateGroupDto with { GroupId = groupId };
        var command = new UpdateGroupCommand(dto);
        var result = await _mediator.Send(command);
        return result;
    }

    [TranslateResultToActionResult]
    [HttpGet("{groupId:long}/balances")]
    public async Task<Result<GetGroupBalancesDto>> GetBalances(
  [FromRoute] int groupId)
    {
        var cmd = new GetGroupBalancesCommand(groupId);
        return await _mediator.Send(cmd);
    }


}
