using System.Text.Json;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.Infrastructure.AuthService;

namespace KasaWGrupie.API.Controllers;

[Route("groups")]
[ApiController]
[FirebaseAuthorize]
public sealed class GroupsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IAuthService _authService;
	public GroupsController(IMediator mediator, IAuthService authService)
	{
		_mediator = mediator;
		_authService = authService;
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

		var userId = await _authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new CreateGroupCommand(userId, createGroupDto, image);
		var result = await _mediator.Send(command);

		return result;
	}

	[TranslateResultToActionResult]
	[HttpGet("{groupId:int}/expenses")]
	public async Task<Result<ICollection<GetExpensesDto>>> GetExpenses([FromRoute] int groupId)
	{
		var userId = await _authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new GetExpensesCommand(userId, groupId);
		var result = await _mediator.Send(command);

		return result;
	}


	[TranslateResultToActionResult]
	[HttpPut("{groupId}")]
	[Consumes("multipart/form-data")]
	public async Task<Result> UpdateGroup([FromRoute] int groupId, IFormCollection formCollection, IFormFile? image)
	{
		var json = formCollection["dto"][0];
		if (json is null)
		{
			return Result.Invalid(new ValidationError("dto", "JSON data required in 'dto' field."));
		}

		var updateGroupDto = JsonSerializer.Deserialize<UpdateGroupDto>(json, JsonOptions);
		if (updateGroupDto is null)
		{
			return Result.Invalid(new ValidationError("dto", "Invalid JSON data."));
		}

		var userId = await _authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new UpdateGroupCommand(userId, groupId, updateGroupDto, image);
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
	[HttpPut("inviteCode")]
	public async Task<Result> RequestToJoinGroupWithInviteCode([FromBody] InviteCodeDto updateInviteCodeDto)
	{
		var userEmail = await _authService.GetEmailFromAuthTokenAsync(HttpContext, HttpContext.RequestAborted);
		var command = new RequestToJoinGroupWithInviteCodeCommand(updateInviteCodeDto, userEmail);
		var result = await _mediator.Send(command);
		return result;
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



    // Add user
    [TranslateResultToActionResult]
    [HttpPut("{groupId:int}/members/{memberId:int}")]
    public async Task<Result> AddUser(
      [FromRoute] int groupId,
      [FromRoute] int memberId)
    {
        var cmd = new AddUserToGroupCommand(groupId, memberId);
        return await _mediator.Send(cmd);
    }

    // Remove user
    [TranslateResultToActionResult]
    [HttpDelete("{groupId:int}/members/{memberId:int}")]
    public async Task<Result> RemoveUser(
      [FromRoute] int groupId,
      [FromRoute] int memberId)
    {
        var cmd = new RemoveUserFromGroupCommand(groupId, memberId);
        return await _mediator.Send(cmd);
    }



	[TranslateResultToActionResult]
	[HttpGet("{groupId}/inviteCode")]
	public async Task<Result<InviteCodeDto>> GetGroupInviteCode(int groupId)
	{
		var command = new GetGroupInviteCodeCommand(groupId);
		var result = await _mediator.Send(command);
		return result;
	}

}
