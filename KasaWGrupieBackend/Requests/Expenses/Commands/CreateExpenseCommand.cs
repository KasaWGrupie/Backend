using Ardalis.Result;
using KasaWGrupie.API.DTOs.Expense;
using MediatR;

namespace KasaWGrupie.API.Requests.Expenses.Commands;

public record CreateExpenseCommand(
	int UserId,
	CreateExpenseDto CreateExpenseDto,
	IFormFile? ExpensePicture
	) : IRequest<Result>;