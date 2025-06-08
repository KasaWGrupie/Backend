using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers;

[Route("expense")]
[ApiController]
[FirebaseAuthorize]
public class ExpenseController(
    IAuthService authService,
    IMediator mediator
    ) : ControllerBase
{
    
    /// <summary>
    /// Create a new expense
    /// </summary>
    [HttpPost]
    [TranslateResultToActionResult]
    public async Task<Result> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new CreateExpenseCommand(userId, createExpenseDto);
        var result = await mediator.Send(command);
        
        return result;
    }

    /// <summary>
    /// Update the details of an existing expense
    /// </summary>
    [HttpPut]
    [TranslateResultToActionResult]
    public async Task<Result> UpdateExpense([FromBody] UpdateExpenseDto updateExpenseDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new UpdateExpenseCommand(userId, updateExpenseDto);
        var result = await mediator.Send(command);
        
        return result;
    }
}