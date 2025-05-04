using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using KasaWGrupie.API.DTOs.FriendRequest;
using KasaWGrupie.API.Requests.FriendRequests.Commands;

namespace KasaWGrupie.API.Controllers;

[Route("users")]
[ApiController]
public class UsersController(
	IAuthService authService,
	IMediator mediator
) : ControllerBase
{
	/// <summary>
	/// Add new User
	/// </summary>
	/// <returns>Successfully inserted new User</returns>
	[HttpPost]
	[TranslateResultToActionResult]
	[FirebaseAuthorize]
	public async Task<Result> CreateUser([FromForm] CreateUserDto createUserDto)
	{
		var authEmail = await authService.GetEmailFromAuthTokenAsync(HttpContext, HttpContext.RequestAborted);
		if (!string.Equals(authEmail, createUserDto.Email))
			return Result.Forbidden("User email does not match.");

		var command = new CreateUserCommand(createUserDto);
		var result = await mediator.Send(command);

		return result;
	}

	[HttpGet("user-groups")]
	[TranslateResultToActionResult]
	public async Task<Result<List<GetUserGroupsDto>>> GetUserGroups([FromQuery] string email)
	{
		var command = new GetUserGroupsCommand(email);
		return await mediator.Send(command);
	}

	[HttpPost("friendRequests")]
	[TranslateResultToActionResult]
	public async Task<Result> AddFriendRequest([FromBody] AddFriendRequestDto addFriendRequestDto)
	{
		var command = new AddFriendRequestCommand(addFriendRequestDto);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpPost("friendRequests/{requestId}")]
	[TranslateResultToActionResult]
	public async Task<Result> ChangeFriendRequestStatus(int requestId, [FromBody] ChangeFriendRequestStatusDto changeFriendRequestStatusDto)
	{
		var command = new ChangeFriendRequestStatusCommand(requestId, changeFriendRequestStatusDto);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpGet("{userId}/friendRequests")]
	[TranslateResultToActionResult]
	public async Task<Result<ICollection<FriendRequestDisplayDto>>> GetFriendRequests(int userId)
	{
		var command = new GetFriendRequestsCommand(userId);
		var result = await mediator.Send(command);
		return result;
	}

}