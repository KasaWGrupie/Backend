// KasaWGrupie.API/DTOs/Users/UserToUserBalanceEntryDto.cs
namespace KasaWGrupie.API.DTOs.Users;

public sealed record UserToUserBalanceEntryDto(
  int GroupId,
  string GroupName,
  decimal Amount,
  string Currency,
  bool IsOwed
);

public sealed record GetUserToUserBalancesDto(
  int UserId,
  int OtherUserId,
  IReadOnlyCollection<UserToUserBalanceEntryDto> Balances
);
