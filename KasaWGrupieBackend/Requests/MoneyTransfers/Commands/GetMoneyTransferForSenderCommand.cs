using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Commands;

public sealed record GetMoneyTransferForSenderCommand(
    int SenderId,
    string? Status = null) : IRequest<Result<List<GetMoneyTransferDto>>>;