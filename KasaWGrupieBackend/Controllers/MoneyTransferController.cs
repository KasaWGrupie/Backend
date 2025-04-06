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
    public async Task<Result> CreateMoneyTransfer([FromForm] CreateMoneyTransferDto createMoneyTransferDto)
    {
        var command = new CreateMoneyTransferCommand(createMoneyTransferDto);
        var result = await mediator.Send(command);
        
        return result;
    }

    [HttpPut("{transferId:int}/status")]
    [TranslateResultToActionResult]
    public async Task<Result> UpdateMoneyTransfer([FromRoute] int transferId, [FromBody] UpdateMoneyTransferDto updateMoneyTransferDto)
    {
        updateMoneyTransferDto = updateMoneyTransferDto with { TransferId = transferId };
        
        var command = new UpdateMoneyTransferCommand(updateMoneyTransferDto);
        var result = await mediator.Send(command);
        
        return result;
    }
    
    
}