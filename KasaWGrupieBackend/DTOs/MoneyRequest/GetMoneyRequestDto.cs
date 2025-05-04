namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record GetMoneyRequestDto(
    int Id,
    int SenderId,
    int RecipientId,
    decimal MoneyValue,
    ICollection<int> Groups,
    string Status,
    DateTime? EndDate
    );