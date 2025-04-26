namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record CreateMoneyRequestDto(
    int SenderId,
    int ReceiverId,
    ICollection<int> GroupIds
    );