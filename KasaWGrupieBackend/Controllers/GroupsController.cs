using System.Text.Json;
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
	
	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

	/// <summary>
	/// Add new group
	/// </summary>
	/// <returns>Successfully inserted new group</returns>
	[TranslateResultToActionResult]
	[HttpPost]
	[Consumes("multipart/form-data")]
	public async Task<Result> CreateGroup(IFormCollection formCollection, IFormFile? image)
	{
		var json = formCollection["dto"][0];
		if (json is null)
		{
			return Result.Invalid(new ValidationError("dto", "JSON data required in 'dto' field."));
		}
		var createGroupDto = JsonSerializer.Deserialize<CreateGroupDto>(json, JsonOptions);
		if (createGroupDto is null)
		{
			return Result.Invalid(new ValidationError("dto", "Invalid JSON data."));
		}
		
		var command = new CreateGroupCommand(createGroupDto, image);
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
    [Consumes("multipart/form-data")]
    public async Task<Result> UpdateGroup(int groupId, IFormCollection formCollection, IFormFile? image)
    {
	    var json = formCollection["dto"][0];
	    if (json is null)
	    {
		    return Result.Invalid(new ValidationError("dto", "JSON data required in 'dto' field."));
	    }
	    var createUserDto = JsonSerializer.Deserialize<UpdateGroupDto>(json, JsonOptions);
	    if (createUserDto is null)
	    {
		    return Result.Invalid(new ValidationError("dto", "Invalid JSON data."));
	    }
	    
        var command = new UpdateGroupCommand(groupId, createUserDto, image);
        var result = await _mediator.Send(command);
        return result;
    }
    
    [TranslateResultToActionResult]
    [HttpPut("{groupId:int}/status")]
    public async Task<Result> ChangeStatus(
  [FromRoute] int groupId,
  [FromBody] ChangeGroupStatusDto dto)
    {
      // merge route + body into one DTO:
        var dtoWithId = dto with { GroupId = groupId };
        var cmd = new ChangeGroupStatusCommand(dtoWithId);
        return await _mediator.Send(cmd);
    }
  
  
    [TranslateResultToActionResult]
    [HttpGet("{groupId:int}")]
    public async Task<Result<GroupDto>> GetGroupById([FromRoute] int groupId)
    {
        var cmd = new GetGroupByIdCommand(groupId);
        return await _mediator.Send(cmd);
    }
    
    [TranslateResultToActionResult]
    [HttpGet("{groupId:int}/balances")]
    public async Task<Result<GetGroupBalancesDto>> GetBalances(
[FromRoute] int groupId)
    {
        var cmd = new GetGroupBalancesCommand(groupId);
        return await _mediator.Send(cmd);
    }





    [TranslateResultToActionResult]
    [HttpGet("{groupId:int}/joinRequests")]
    public async Task<Result<GetGroupJoinRequestsDto>> GetJoinRequests(
  [FromRoute] int groupId)
  => await _mediator.Send(new GetGroupJoinRequestsCommand(groupId));


    [TranslateResultToActionResult]
    [HttpPut("{groupId:int}/joinRequests/{requestId:int}")]
    public async Task<Result> ChangeJoinRequestStatus(
  [FromRoute] int groupId,
  [FromRoute] int requestId,
  [FromBody] ChangeJoinRequestStatusDto dto)
    {
        var cmd = new ChangeJoinRequestStatusCommand(groupId, requestId, dto.Status);
        return await _mediator.Send(cmd);
    }

}
