using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Commands;

public sealed record CreateMoneyTransferCommand(
    int UserId,
    CreateMoneyTransferDto CreateMoneyTransferDto
    ) : IRequest<Result>;
