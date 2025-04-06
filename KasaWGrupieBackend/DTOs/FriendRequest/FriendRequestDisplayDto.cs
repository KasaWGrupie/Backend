namespace KasaWGrupie.API.DTOs.FriendRequest;

public sealed record FriendRequestDisplayDto(
	int Id,
	int SenderId,
	int ReceiverId,
	string SenderName,
	string SenderProfilePictureUrl);