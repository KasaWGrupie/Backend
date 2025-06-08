namespace KasaWGrupie.API.DTOs.Expense;

public sealed record ExpenseParticipantDto(
	int UserId,
	string? Username,
	decimal Amount
);