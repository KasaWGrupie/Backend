using KasaWGrupie.API.DTOs.Users;
using MediatR;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.Users.Commands;
public sealed record UpdateUserProfilePictureCommand(
	int Id,
	IFormFile? ProfilePicture
) : IRequest<Result>;
