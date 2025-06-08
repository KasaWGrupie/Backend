using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.Expense;
using KasaWGrupie.API.Requests.Expenses.Commands;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
	[Consumes("multipart/form-data")]
	public async Task<Result> CreateExpense(IFormCollection formCollection, IFormFile? expensePicture)
	{
		var json = formCollection["dto"][0];

		if (json is null)
		{
			return Result.Invalid(new ValidationError("dto", "JSON data required in 'dto' field."));
		}
		var createExpenseDto = JsonSerializer.Deserialize<CreateExpenseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (createExpenseDto is null)
		{
			return Result.Invalid(new ValidationError("dto", "Invalid JSON data."));
		}

		var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
		var command = new CreateExpenseCommand(userId, createExpenseDto, expensePicture);
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