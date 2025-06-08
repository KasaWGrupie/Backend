namespace KasaWGrupie.API.DTOs.FriendRequest;

public sealed record SentFriendRequestDisplayDto(
	int Id,
	int SenderId,
	int ReceiverId,
	string ReceiverName,
	string ReceiverProfilePictureUrl);
