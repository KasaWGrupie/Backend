namespace KasaWGrupie.API.DTOs.MoneyTransfer;

public sealed record UpdateMoneyTransferDto(
    int TransferId,
    string Status
);