namespace KasaWGrupie.API.DTOs.MoneyTransfer;

public record GetMoneyTransferDto(
    int Id,
    int SenderId,
    int RecipientId,
    decimal Amount,
    int GroupId,
    DateTime? EndDate,
    string Status
    );