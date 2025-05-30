using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Groups;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class GetGroupByIdHandler
  : IRequestHandler<GetGroupByIdCommand, Result<GroupDto>>
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IValidator<GetGroupByIdCommand> _validator;

    public GetGroupByIdHandler(
      IRepositoryBase<Group> groupRepo,
      IValidator<GetGroupByIdCommand> validator)
    {
        _groupRepo = groupRepo;
        _validator = validator;
    }

    public async Task<Result<GroupDto>> Handle(
      GetGroupByIdCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Fetch group and include members (use your spec or explicit include)
        var spec = new GroupByIdWithMembersSpec(request.GroupId);
        var group = await _groupRepo.FirstOrDefaultAsync(spec, cancellationToken);
        if (group is null)
            return Result.NotFound("Group not found.");

        // 3) Map to DTO
        var dto = new GroupDto(
          group.Id,
          group.Name,
          group.Description,
          group.PictureUrl,
          group.Currency.Name,
          group.Admin.Id,
          group.Status.ToString(),                              // e.g. "open"
          group.Members.Select(m => m.Id).ToList()
        );

        return Result.Success(dto);
    }
}
