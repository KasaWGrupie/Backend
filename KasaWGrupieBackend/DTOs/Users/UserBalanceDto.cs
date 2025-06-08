namespace KasaWGrupie.API.DTOs.Users;

public sealed record UserBalanceEntryDto(
  int UserId,   // the counterparty’s user ID
  decimal Amount  // total owed
);

public sealed record GetUserBalancesDto(
  int UserId,
  IReadOnlyCollection<UserBalanceEntryDto> OwedByOthers,
  IReadOnlyCollection<UserBalanceEntryDto> OwesToOthers
);
