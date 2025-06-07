using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers;

[Route("moneyTransfer")]
[ApiController]
public class MoneyTransferController(
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
        var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
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
        var command = new UpdateMoneyTransferCommand(transferId, updateMoneyTransferDto);
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
        var command = new GetMoneyTransferForRecipientCommand(recipientId, status);
        var result = await mediator.Send(command);

        return result;
    }
    
    
}