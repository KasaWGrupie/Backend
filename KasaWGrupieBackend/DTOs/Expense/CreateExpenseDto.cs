namespace KasaWGrupie.API.DTOs.Expense;

public sealed record CreateExpenseDto(
	int GroupId,
	int PaidBy,
	string ExpenseName,
	string Description,
	decimal Amount,
	DateTime Date,
	List<ExpenseParticipantDto> Participants,
	string DivisionMethod
);