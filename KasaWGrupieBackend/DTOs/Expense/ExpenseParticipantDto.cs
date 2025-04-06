namespace KasaWGrupie.API.DTOs.Expense;

public sealed record ExpenseParticipantDto(
    int UserId,
    string Username, // potrzebujemy tu tego w ogóle?
    decimal Amount
);