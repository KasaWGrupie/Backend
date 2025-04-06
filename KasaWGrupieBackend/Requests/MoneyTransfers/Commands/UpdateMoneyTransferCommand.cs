using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Commands;

public sealed record UpdateMoneyTransferCommand(
    UpdateMoneyTransferDto UpdateMoneyTransferDto
) : IRequest<Result>;