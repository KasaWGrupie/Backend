using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyRequest;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Commands;

public record GetMoneyRequestForReceiverCommand(
    int ReceiverId,
    string? Status = null) : IRequest<Result<List<GetMoneyRequestDto>>>;