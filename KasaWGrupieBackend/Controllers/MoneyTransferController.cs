using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers;

[Route("moneyTransfer")]
[ApiController]
[FirebaseAuthorize]
public class MoneyTransferController(
    IAuthService authService,
    IMediator mediator
) : ControllerBase
{

    /// <summary>
    /// Initiates a money transfer between users within a group.
    /// </summary>
    /// <returns>Successfully inserted new Money Transfer</returns>
    [HttpPost]
    [TranslateResultToActionResult]
    public async Task<Result> CreateMoneyTransfer([FromBody] CreateMoneyTransferDto createMoneyTransferDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new CreateMoneyTransferCommand(userId, createMoneyTransferDto);
        var result = await mediator.Send(command);
        
        return result;
    }

    /// <summary>
    /// Changes the status of a money transfer to either <c>confirmed</c> or <c>rejected</c>.
    /// </summary>
    /// <returns>Money transfer status updated successfully</returns>
    [HttpPut("{transferId:int}/status")]
    [TranslateResultToActionResult]
    public async Task<Result> UpdateMoneyTransfer([FromRoute] int transferId, [FromBody] UpdateMoneyTransferDto updateMoneyTransferDto)
    {
        var userId = await authService.GetUserIdFromAuthTokenAsync(HttpContext);
        var command = new UpdateMoneyTransferCommand(userId, transferId, updateMoneyTransferDto);
        var result = await mediator.Send(command);
        
        return result;
    }
    
    
    /// <summary>
    /// Get all transfers sent by a given user
    /// </summary>
    [HttpGet("findBySender")]
    [TranslateResultToActionResult]
    public async Task<Result<List<GetMoneyTransferDto>>> GetMoneyTransferBySender(
        [FromQuery] int senderId,
        [FromQuery] string? status = null)
    {
        if (senderId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
        {
            return Result.Forbidden("Cannot get transfers sent by another user.");
        }
        
        var command = new GetMoneyTransferForSenderCommand(senderId, status);
        var result = await mediator.Send(command);

        return result;
    }

    /// <summary>
    /// Get all transfers received by a given user
    /// </summary>
    [HttpGet("findByRecipient")]
    [TranslateResultToActionResult]
    public async Task<Result<List<GetMoneyTransferDto>>> GetMoneyTransferByReceiver(
        [FromQuery] int recipientId,
        [FromQuery] string? status = null)
    {
        if (recipientId != await authService.GetUserIdFromAuthTokenAsync(HttpContext))
        {
            return Result.Forbidden("Cannot get transfers received by another user.");
        }
        
        var command = new GetMoneyTransferForRecipientCommand(recipientId, status);
        var result = await mediator.Send(command);

        return result;
    }
    
    
}