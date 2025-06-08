using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Tests.Factories
{
	public static class UserFactory
	{
		public static User Create(
			int id = 1,
			string email = "user@example.com",
			string name = "Test User",
			string profilePictureUrl = "http://example.com/profile.jpg")
		{
			return new User
			{
				Id = id,
				Name = name,
				Email = email,
				ProfilePictureUrl = profilePictureUrl,
				Friends = new List<User>(),
				Groups = new List<Group>(),
				AdministratedGroups = new List<Group>(),
				SentPayRequests = new List<PayRequest>(),
				RecievedPayRequests = new List<PayRequest>(),
				SentFriendRequests = new List<FriendRequest>(),
				RecievedFriendRequests = new List<FriendRequest>()
			};
		}
	}
}
