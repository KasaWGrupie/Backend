using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Users;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ardalis.Specification;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class GetUserBalancesHandler
  : IRequestHandler<GetUserBalancesCommand, Result<GetUserBalancesDto>>
{
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IMediator _mediator;
    private readonly IValidator<GetUserBalancesCommand> _validator;

    public GetUserBalancesHandler(
      IRepositoryBase<User> userRepo,
      IMediator mediator,
      IValidator<GetUserBalancesCommand> validator)
    {
        _userRepo = userRepo;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<Result<GetUserBalancesDto>> Handle(
      GetUserBalancesCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Ensure user exists, load their groups
        var spec = new UserWithGroupsSpec(request.UserId);
        var user = await _userRepo.FirstOrDefaultAsync(spec, cancellationToken);
        if (user is null)
            return Result.NotFound("User not found.");

        // 3) For each group the user is in, fetch balances
        var allOwedBy = new List<UserBalanceEntryDto>();
        var allOwesTo = new List<UserBalanceEntryDto>();

        foreach (var group in user.Groups)
        {
            var groupResult = await _mediator.Send(
              new KasaWGrupie.API.Requests.Groups.Commands.GetGroupBalancesCommand(group.Id),
              cancellationToken);

            if (groupResult.Status != ResultStatus.Ok)
                continue; // skip any missing groups

            var balances = groupResult.Value!.Balances;

            // Those who owe *this* user:
            var owedBy = balances
              .Where(b => b.ToUserId == request.UserId)
              .Select(b => new UserBalanceEntryDto(b.FromUserId, b.Amount));

            // Those to whom *this* user owes:
            var owesTo = balances
              .Where(b => b.FromUserId == request.UserId)
              .Select(b => new UserBalanceEntryDto(b.ToUserId, b.Amount));

            allOwedBy.AddRange(owedBy);
            allOwesTo.AddRange(owesTo);
        }

        // 4) Aggregate amounts per counterparty
        List<UserBalanceEntryDto> aggregate(IEnumerable<UserBalanceEntryDto> list) =>
          list
          .GroupBy(x => x.UserId)
          .Select(g => new UserBalanceEntryDto(g.Key, g.Sum(x => x.Amount)))
          .ToList();

        var owedByOthers = aggregate(allOwedBy);
        var owesToOthers = aggregate(allOwesTo);

        // 5) Return
        var dto = new GetUserBalancesDto(
          request.UserId,
          owedByOthers,
          owesToOthers
        );
        return Result.Success(dto);
    }
}
