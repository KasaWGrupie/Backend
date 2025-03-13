using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

public class JoinRequest : EntityBase
{
	public int RequestingUserId { get; set; }
	public required User RequestingUser { get; set; }
	public int GroupId { get; set; }
	public required Group Group { get; set; }
	public JoinRequestStatus Status { get; set; }
}