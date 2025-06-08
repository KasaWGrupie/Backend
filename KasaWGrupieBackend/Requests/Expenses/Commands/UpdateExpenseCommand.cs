using Ardalis.Result;
using KasaWGrupie.API.DTOs.Expense;
using MediatR;

namespace KasaWGrupie.API.Requests.Expenses.Commands;

public record UpdateExpenseCommand(
    int UserId,
    UpdateExpenseDto UpdateExpenseDto
    ) : IRequest<Result>;
    
