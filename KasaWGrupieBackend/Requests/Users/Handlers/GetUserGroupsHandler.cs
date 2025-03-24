using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.ImageService;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Groups;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class GetUserGroupsHandler : IRequestHandler<GetUserGroupsCommand, Result<List<GetUserGroupsDto>>>
{
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IValidator<GetUserGroupsCommand> _validator;

    public GetUserGroupsHandler(
        IRepositoryBase<Group> groupRepository,
        IRepositoryBase<User> userRepository,
        IValidator<GetUserGroupsCommand> validator)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<List<GetUserGroupsDto>>> Handle(GetUserGroupsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }

        var user = await _userRepository.FirstOrDefaultAsync(new UserByEmailSpecification(request.UserEmail), cancellationToken);
        if (user == null)
            return Result.NotFound();

        var groups = await _groupRepository.ListAsync(new GroupsByMemberEmailSpecification(user.Email), cancellationToken);


        var dtos = groups.Select(g => new GetUserGroupsDto(
            g.Id,
            g.Name,
            g.Currency?.Name ?? "N/A",
            g.Admin?.Email ?? "N/A"
        )).ToList();

        return Result.Success(dtos);
    }


}
