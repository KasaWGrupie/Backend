namespace KasaWGrupie.API.DTOs.Expense;

public record UpdateExpenseDto(
    int ExpenseId,
    int? PaidBy,
    string? ExpenseName,
    string? ExpensePictureUri,
    string? Description,
    decimal? Amount,
    DateTime? Date,
    List<ExpenseParticipantDto>? Participants,
    string? DivisionMethod
);