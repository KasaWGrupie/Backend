using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers;

[Route("moneyRequest")]
[ApiController]
[FirebaseAuthorize]
public class MoneyRequestController(
    IAuthService authService,
    IMediator mediator
) : ControllerBase
{
    /// <summary>
    /// Create a new money transfer
    /// </summary>
    /// <returns>Money transfer created successfully</returns>
    [HttpPost]
    [TranslateResultToActionResult]
    public async Task<Result> CreateMoneyRequest([FromForm] CreateMoneyRequestDto createMoneyRequestDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new CreateMoneyRequestCommand(userId, createMoneyRequestDto);
        var result = await mediator.Send(command);
        
        return result;
    }
    
    /// <summary>
    /// Update money request status
    /// </summary>
    /// <returns>Successfully updated money request status</returns>
    [HttpPut("{requestId:int}")]
    [TranslateResultToActionResult]
    public async Task<Result> UpdateMoneyRequestStatus([FromRoute] int requestId, [FromBody] UpdateMoneyRequestStatusDto updateMoneyRequestStatusDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new UpdateMoneyRequestStatusCommand(userId, requestId, updateMoneyRequestStatusDto);
        var result = await mediator.Send(command);
        
        return result;
    }

    /// <summary>
    /// Get all requests sent by a given user
    /// </summary>
    [HttpGet("findBySender")]
    [TranslateResultToActionResult]
    public async Task<Result<List<GetMoneyRequestDto>>> GetMoneyRequestBySender(
        [FromQuery] int senderId,
        [FromQuery] string? status = null)
    {
        if (senderId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
        {
            return Result.Forbidden("Cannot get requests sent by another user.");
        }
        var command = new GetMoneyRequestForSenderCommand(senderId, status);
        var result = await mediator.Send(command);
        
        return result;
    }
    
    /// <summary>
    /// Get all requests received by a given user
    /// </summary>
    [HttpGet("findByRecipient")]
    [TranslateResultToActionResult]
    public async Task<Result<List<GetMoneyRequestDto>>> GetMoneyRequestByReceiver(
        [FromQuery] int recipientId,
        [FromQuery] string? status = null)
    {
        if (recipientId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
        {
            return Result.Forbidden("Cannot get requests received by another user.");
        }
        var command = new GetMoneyRequestForReceiverCommand(recipientId, status);
        var result = await mediator.Send(command);
        
        return result;
    }
}