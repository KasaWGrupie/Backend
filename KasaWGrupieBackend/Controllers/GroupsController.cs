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

	/// <summary>
	/// Add new group
	/// </summary>
	/// <returns>Successfully inserted new group</returns>
	[TranslateResultToActionResult]
	[HttpPost]
	public async Task<Result> CreateGroup([FromForm] CreateGroupDto createGroupDto)
	{
		var userEmail = await _authService.GetEmailFromAuthTokenAsync(HttpContext, HttpContext.RequestAborted);
		var command = new CreateGroupCommand(userEmail, createGroupDto);
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
    public async Task<Result> UpdateGroup([FromRoute] int groupId, [FromForm] UpdateGroupDto updateGroupDto)
    {
	    var userId = await _authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new UpdateGroupCommand(userId, groupId, updateGroupDto);
        var result = await _mediator.Send(command);
        return result;
    }



}
