namespace KasaWGrupie.API.DTOs.FriendRequest;

public sealed class AddFriendRequestDto
{
	public int SenderId { get; set; }
	public int ReceiverId { get; set; }
}