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

	[HttpGet("email/{userEmail}")]
	[TranslateResultToActionResult]
	public async Task<Result<GetUserDto>> GetUserByEmail(string userEmail)
	{
		var command = new GetUserByEmailCommand(userEmail);
		return await mediator.Send(command);
	}

	[HttpGet("{id}")]
	[TranslateResultToActionResult]
	public async Task<Result<GetUserDto>> GetUserById(int id)
	{
		var command = new GetUserByIdCommand(id);
		return await mediator.Send(command);
	}

	[HttpDelete("{id}")]
	[TranslateResultToActionResult]
	public async Task<Result> DeleteUser(int id)
	{
		var command = new DeleteUserCommand(id);
		return await mediator.Send(command);
	}

	[HttpPut("name/{id}")]
	[TranslateResultToActionResult]
	public async Task<Result> UpdateUserName(int id, [FromBody] UpdateUserNameDto updateUserNameDto)
	{
		var command = new UpdateUserNameCommand(id, updateUserNameDto);
		return await mediator.Send(command);
	}

	[HttpPut("profilePicture/{id}")]
	[TranslateResultToActionResult]
	public async Task<Result> UpdateUserProfilePicture(int id, [FromForm] UpdateUserProfilePictureDto updateUserProfilePictureDto)
	{
		var command = new UpdateUserProfilePictureCommand(id, updateUserProfilePictureDto);
		return await mediator.Send(command);
	}

	[HttpGet("user-groups")]
	[TranslateResultToActionResult]
	public async Task<Result<List<GetUserGroupsDto>>> GetUserGroups([FromQuery] string email)
	{
		var command = new GetUserGroupsCommand(email);
		return await mediator.Send(command);
	}

	[HttpGet("friends/{userId}")]
	[TranslateResultToActionResult]
	public async Task<Result<ICollection<GetUserDto>>> GetFriends(int userId)
	{
		var command = new GetFriendsCommand(userId);
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