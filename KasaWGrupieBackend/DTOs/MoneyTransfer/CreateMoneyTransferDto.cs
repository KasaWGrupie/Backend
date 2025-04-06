namespace KasaWGrupie.API.DTOs.MoneyTransfer;

public sealed record CreateMoneyTransferDto(
    int SenderId,
    int RecipientId,
    decimal Amount,
    int GroupId
);