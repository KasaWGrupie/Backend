// KasaWGrupie.API/Requests/Groups/Handlers/GetGroupJoinRequestsHandler.cs
using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.Groups;
using KasaWGrupie.Infrastructure.AuthService;        // for IAuthService
using Microsoft.AspNetCore.Http;                   // for IHttpContextAccessor
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;

public class GetGroupJoinRequestsHandler
  : IRequestHandler<GetGroupJoinRequestsCommand, Result<GetGroupJoinRequestsDto>>
{
    readonly IRepositoryBase<Group> _groupRepo;
    readonly IRepositoryBase<JoinRequest> _joinReqRepo;
    readonly IRepositoryBase<User> _userRepo;
    readonly IValidator<GetGroupJoinRequestsCommand> _validator;
    readonly IAuthService _authService;
    readonly IHttpContextAccessor _httpContext;

    public GetGroupJoinRequestsHandler(
      IRepositoryBase<Group> groupRepo,
      IRepositoryBase<JoinRequest> joinReqRepo,
      IRepositoryBase<User> userRepo,
      IValidator<GetGroupJoinRequestsCommand> validator,
      IAuthService authService,
      IHttpContextAccessor httpContextAccessor)
    {
        _groupRepo = groupRepo;
        _joinReqRepo = joinReqRepo;
        _userRepo = userRepo;
        _validator = validator;
        _authService = authService;
        _httpContext = httpContextAccessor;
    }

    public async Task<Result<GetGroupJoinRequestsDto>> Handle(
      GetGroupJoinRequestsCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Load group (to check admin)
        var groupSpec = new GroupByIdWithMembersSpec(request.GroupId);
        var group = await _groupRepo.FirstOrDefaultAsync(groupSpec, cancellationToken);
        if (group is null)
            return Result.NotFound("Group not found.");

        // 3) Authenticate & get current user's email
        var email = await _authService.GetEmailFromAuthTokenAsync(
          _httpContext.HttpContext!, cancellationToken);

        // 4) Lookup that user in your Core
        var user = await _userRepo.FirstOrDefaultAsync(
          new KasaWGrupie.Persistence.Specifications.Users.UserByEmailSpecification(email),
          cancellationToken);

        if (user is null || user.Id != group.Admin.Id)
            return Result.Unauthorized();

        // 5) Load join-requests
        var jrSpec = new JoinRequestsByGroupSpec(request.GroupId);
        var joins = await _joinReqRepo.ListAsync(jrSpec, cancellationToken);

        // 6) Map DTOs
        var dtos = joins.Select(j => new JoinRequestDto(
          j.Id,
          j.RequestingUserId,
          j.RequestingUser.Name,
          j.Status              // enum
        )).ToList();

        return Result.Success(new GetGroupJoinRequestsDto(request.GroupId, dtos));
    }
}
