using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Commands;

public sealed record UpdateMoneyTransferCommand(
    int UserId,
    int TransferId,
    UpdateMoneyTransferDto UpdateMoneyTransferDto
) : IRequest<Result>;