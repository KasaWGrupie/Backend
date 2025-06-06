using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class ChangeGroupStatusHandler
  : IRequestHandler<ChangeGroupStatusCommand, Result>
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IValidator<ChangeGroupStatusCommand> _validator;

    public ChangeGroupStatusHandler(
      IRepositoryBase<Group> groupRepo,
      IValidator<ChangeGroupStatusCommand> validator)
    {
        _groupRepo = groupRepo;
        _validator = validator;
    }

    public async Task<Result> Handle(
      ChangeGroupStatusCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate input
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        var dto = request.StatusDto;

        // 2) Load group
        var group = await _groupRepo.GetByIdAsync(dto.GroupId, cancellationToken);
        if (group is null)
            return Result.NotFound("Group not found.");

        // 3) Change status & persist
        group.Status = dto.Status;
        await _groupRepo.UpdateAsync(group, cancellationToken);
        await _groupRepo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
