namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record CreateMoneyRequestDto(
    int SenderId,
    int ReceiverId,
    string Currency,
    ICollection<int> Groups
    );