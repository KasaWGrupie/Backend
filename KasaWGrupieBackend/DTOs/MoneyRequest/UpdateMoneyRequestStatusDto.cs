namespace KasaWGrupie.API.DTOs.MoneyRequest;

public record UpdateMoneyRequestStatusDto(
    int RequestId,
    string Status
    );