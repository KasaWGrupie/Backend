using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers;

[Route("expense")]
[ApiController]
public class ExpenseController(
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
        var command = new CreateExpenseCommand(createExpenseDto);
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
        var command = new UpdateExpenseCommand(updateExpenseDto);
        var result = await mediator.Send(command);
        
        return result;
    }
}