using Ardalis.Result;
using KasaWGrupie.API.DTOs.Groups;
using MediatR;

namespace KasaWGrupie.API.Requests.Groups.Commands;

public sealed record GetExpensesCommand(
    int UserId,
    int GroupId
    ) : IRequest<Result<ICollection<GetExpensesDto>>>;