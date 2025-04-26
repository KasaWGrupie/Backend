namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record GetMoneyRequestDto(
    int Id,
    int SenderId,
    int ReceiverId,
    decimal MoneyValue,
    ICollection<int> GroupIds,
    string Status,
    DateTime? EndDate
    );