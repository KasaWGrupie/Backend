namespace KasaWGrupie.API.DTOs.Users;

public sealed record UpdateUserProfilePictureDto(
	IFormFile? ProfilePicture
	);