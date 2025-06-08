namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record GetMoneyRequestDto(
    int Id,
    int SenderId,
    int RecipientId,
    decimal MoneyValue,
    string Currency,
    ICollection<int> Groups,
    string Status,
    DateTime? EndDate
    );