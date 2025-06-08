// KasaWGrupie.API/Requests/Users/Handlers/GetUserBalancesWithUserHandler.cs
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

public class GetUserBalancesWithUserHandler
  : IRequestHandler<GetUserBalancesWithUserCommand, Result<GetUserToUserBalancesDto>>
{
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IMediator _mediator;
    private readonly IValidator<GetUserBalancesWithUserCommand> _validator;

    public GetUserBalancesWithUserHandler(
      IRepositoryBase<User> userRepo,
      IMediator mediator,
      IValidator<GetUserBalancesWithUserCommand> validator)
    {
        _userRepo = userRepo;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<Result<GetUserToUserBalancesDto>> Handle(
      GetUserBalancesWithUserCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate
        var val = await _validator.ValidateAsync(request, cancellationToken);
        if (!val.IsValid)
            return Result.Invalid(val.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Load both users with their Groups (+Currency)
        var spec = new UserWithGroupsAndCurrencySpec(request.UserId);
        var user = await _userRepo.FirstOrDefaultAsync(spec, cancellationToken);
        if (user is null) return Result.NotFound("User not found.");

        spec = new UserWithGroupsAndCurrencySpec(request.OtherUserId);
        var other = await _userRepo.FirstOrDefaultAsync(spec, cancellationToken);
        if (other is null) return Result.NotFound("Other user not found.");

        // 3) Find common groups
        var common = user.Groups.Select(g => g.Id)
          .Intersect(other.Groups.Select(g => g.Id))
          .ToList();
        if (!common.Any())
            return Result.Success(new GetUserToUserBalancesDto(request.UserId, request.OtherUserId, Array.Empty<UserToUserBalanceEntryDto>()));

        var entries = new List<UserToUserBalanceEntryDto>();

        // 4) For each common group, fetch group-level balances and pick the record
        foreach (var groupId in common)
        {
            var grpResult = await _mediator.Send(
              new KasaWGrupie.API.Requests.Groups.Commands.GetGroupBalancesCommand(groupId),
              cancellationToken);
            if (grpResult.Status != ResultStatus.Ok)
                continue; // skip if group missing

            var balances = grpResult.Value!.Balances;
            // pick the pair
            var rec = balances.FirstOrDefault(b =>
              (b.FromUserId == request.UserId && b.ToUserId == request.OtherUserId) ||
              (b.FromUserId == request.OtherUserId && b.ToUserId == request.UserId));
            if (rec == null) continue;

            // get group name & currency from user.Groups
            var grp = user.Groups.First(g => g.Id == groupId);

            var isOwed = rec.ToUserId == request.UserId;
            entries.Add(new UserToUserBalanceEntryDto(
              groupId,
              grp.Name,
              rec.Amount,
              grp.Currency.Name,
              isOwed
            ));
        }

        return Result.Success(new GetUserToUserBalancesDto(
          request.UserId,
          request.OtherUserId,
          entries
        ));
    }
}
