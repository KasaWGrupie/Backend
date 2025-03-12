using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Core.Entities;

class JoinRequest : EntityBase
{
	public required User RequestingUser { get; set; }
	public required Group Group { get; set; }
	public JoinRequestStatus Status { get; set; }
}