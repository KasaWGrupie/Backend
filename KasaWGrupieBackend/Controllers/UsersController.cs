using System.Text.Json;
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
	[Consumes("multipart/form-data")]
	public async Task<Result> CreateUser(IFormCollection formCollection, IFormFile? profilePicture)
	{
		var json = formCollection["dto"][0];
		if (json is null)
		{
			return Result.Invalid(new ValidationError(nameof(formCollection), "Invalid JSON data."));
		}
		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var createUserDto = JsonSerializer.Deserialize<CreateUserDto>(json, options);
		if (createUserDto == null)
		{
			return Result.Invalid(new ValidationError("dto", "Invalid JSON data."));
		}

		var authEmail = await authService.GetEmailFromAuthTokenAsync(HttpContext, HttpContext.RequestAborted);
		if (!string.Equals(authEmail, createUserDto.Email))
			return Result.Forbidden("User email does not match.");

		var command = new CreateUserCommand(createUserDto, profilePicture);
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
		if (id != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
		{
			return Result.Forbidden("Cannot delete another user's account.");
		}
		var command = new DeleteUserCommand(id);
		return await mediator.Send(command);
	}

	[HttpPut("name/{id}")]
	[TranslateResultToActionResult]
	public async Task<Result> UpdateUserName(int id, [FromBody] UpdateUserNameDto updateUserNameDto)
	{
		if (id != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
		{
			return Result.Forbidden("Cannot update another user's name.");
		}
		var command = new UpdateUserNameCommand(id, updateUserNameDto);
		return await mediator.Send(command);
	}

	[HttpPut("profilePicture/{id}")]
	[TranslateResultToActionResult]
	public async Task<Result> UpdateUserProfilePicture(int id, [FromForm] UpdateUserProfilePictureDto updateUserProfilePictureDto)
	{
		if (id != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
		{
			return Result.Forbidden("Cannot update another user's profile picture.");
		}
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
		// inni użytkownicy mogą czy nie?
		var command = new GetFriendsCommand(userId);
		return await mediator.Send(command);
	}

	[HttpPost("friendRequests")]
	[TranslateResultToActionResult]
	public async Task<Result> AddFriendRequest([FromBody] AddFriendRequestDto addFriendRequestDto)
	{
		var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new AddFriendRequestCommand(userId, addFriendRequestDto);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpPost("friendRequests/{requestId}")]
	[TranslateResultToActionResult]
	public async Task<Result> ChangeFriendRequestStatus(int requestId, [FromBody] ChangeFriendRequestStatusDto changeFriendRequestStatusDto)
	{
		var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new ChangeFriendRequestStatusCommand(userId, requestId, changeFriendRequestStatusDto);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpGet("{userId}/receivedFriendRequests")]
	[TranslateResultToActionResult]
	public async Task<Result<ICollection<RecievedFriendRequestDisplayDto>>> GetRecievedFriendRequests(int userId)
	{
		//if (userId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
		//{
		//	return Result.Forbidden("Cannot get friend requests of another user.");
		//}
		var command = new GetFriendRequestsCommand(userId);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpGet("{userId}/sentFriendRequests")]
	[TranslateResultToActionResult]
	public async Task<Result<ICollection<SentFriendRequestDisplayDto>>> GetSentFriendRequests(int userId)
	{
		//if (userId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
		//{
		//	return Result.Forbidden("Cannot get friend requests of another user.");
		//}
		var command = new GetSentFriendRequestsCommand(userId);
		var result = await mediator.Send(command);
		return result;
	}

	[HttpGet("email/search/{query}")]
	[TranslateResultToActionResult]
	public async Task<Result<List<GetUserDto>>> SearchUsersByEmail([FromRoute] string query)
	{
		var command = new SearchUsersByPartialEmailCommand(query);
		var result = await mediator.Send(command);
		return result;
	}

}