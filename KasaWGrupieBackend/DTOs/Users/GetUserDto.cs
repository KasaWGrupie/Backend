namespace KasaWGrupie.API.DTOs.Users;

public class GetUserDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string ProfilePictureUrl { get; set; } = string.Empty;
}