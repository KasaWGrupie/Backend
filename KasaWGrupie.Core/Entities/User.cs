namespace KasaWGrupie.Core.Entities;

class User : EntityBase
{
	public required string Name { get; set; }
	public required string Email { get; set; }
	public required string ProfilePictureUrl { get; set; }
	public ICollection<User> Friends { get; set; }
	public ICollection<Group> Groups { get; set; } = [];
	public ICollection<Group> AdministratedGroups { get; set; } = [];
	public ICollection<PayRequest> SentPayRequests { get; set; } = [];
	public ICollection<PayRequest> RecievedPayRequests { get; set; } = [];
	public ICollection<FriendRequest> SentFriendRequests { get; set; } = [];
	public ICollection<FriendRequest> RecievedFriendRequests { get; set; } = [];
	public ICollection<JoinRequests> RecievedJoinRequests { get; set; } = [];
}