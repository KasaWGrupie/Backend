using Ardalis.Result;
using KasaWGrupie.API.DTOs.MoneyRequest;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Commands;

public record CreateMoneyRequestCommand(
    CreateMoneyRequestDto CreateMoneyRequestDto
    ) : IRequest<Result>;