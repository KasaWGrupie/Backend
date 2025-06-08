using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class ChangeJoinRequestStatusHandler
  : IRequestHandler<ChangeJoinRequestStatusCommand, Result>
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IRepositoryBase<JoinRequest> _joinReqRepo;
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IValidator<ChangeJoinRequestStatusCommand> _validator;
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContext;

    public ChangeJoinRequestStatusHandler(
      IRepositoryBase<Group> groupRepo,
      IRepositoryBase<JoinRequest> joinReqRepo,
      IRepositoryBase<User> userRepo,
      IValidator<ChangeJoinRequestStatusCommand> validator,
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

    public async Task<Result> Handle(
      ChangeJoinRequestStatusCommand request,
      CancellationToken cancellationToken)
    {
        // 1) Validate command
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Load group for admin check
        var group = await _groupRepo.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
            return Result.NotFound("Group not found.");

        // 3) Authenticate & ensure admin
        var email = await _authService.GetEmailFromAuthTokenAsync(
          _httpContext.HttpContext!, cancellationToken);
        var user = await _userRepo.FirstOrDefaultAsync(
          new KasaWGrupie.Persistence.Specifications.Users.UserByEmailSpecification(email),
          cancellationToken);
        if (user is null || user.Id != group.Admin.Id)
            return Result.Unauthorized();

        // 4) Load join request
        var jr = await _joinReqRepo.GetByIdAsync(request.RequestId, cancellationToken);
        if (jr is null || jr.GroupId != request.GroupId)
            return Result.NotFound("Join request not found.");

        // 5) Update and persist
        jr.Status = request.Status;
        await _joinReqRepo.UpdateAsync(jr, cancellationToken);
        await _joinReqRepo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
