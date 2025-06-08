namespace KasaWGrupie.API.DTOs.FriendRequest;

public sealed record RecievedFriendRequestDisplayDto(
	int Id,
	int SenderId,
	int ReceiverId,
	string SenderName,
	string SenderProfilePictureUrl);