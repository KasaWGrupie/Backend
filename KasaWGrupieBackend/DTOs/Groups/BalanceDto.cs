namespace KasaWGrupie.API.DTOs.Groups
{
    public sealed record BalanceDto
    (
        int FromUserId,
        int ToUserId,
        decimal Amount
    );

    public sealed record GetGroupBalancesDto
    (
        int GroupId,                           
        IReadOnlyCollection<BalanceDto> Balances
    );

}
