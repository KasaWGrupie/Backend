namespace KasaWGrupie.API.DTOs.Groups;

public sealed record CreateGroupDto(
	string Name,
	string Description,
	string Currency,
	int AdminId,
	ICollection<int> Members
	);