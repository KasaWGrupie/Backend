using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Commands;

public sealed record GetMoneyTransferForRecipientCommand(
    int RecipientId,
    string? Status = null) : IRequest<Result<List<GetMoneyTransferDto>>>;