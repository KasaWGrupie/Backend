using KasaWGrupie.API.DTOs.Expense;

namespace KasaWGrupie.API.DTOs.Groups;

public sealed record GetExpensesDto(
    int ExpenseId,
    int PaidBy,
    string ExpenseName,
    string ExpensePictureUri,
    string Description,
    decimal Amount,
    DateTime Date,
    List<ExpenseParticipantDto> Participants,
    string DivisionMethod
);