using Ardalis.Result;
using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.AuthService;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using KasaWGrupie.Persistence.Specifications.Groups;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class RemoveUserFromGroupHandler
  : IRequestHandler<RemoveUserFromGroupCommand, Result>
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IValidator<RemoveUserFromGroupCommand> _validator;
    private readonly IAuthService _auth;
    private readonly IHttpContextAccessor _http;

    public RemoveUserFromGroupHandler(
      IRepositoryBase<Group> groupRepo,
      IRepositoryBase<User> userRepo,
      IValidator<RemoveUserFromGroupCommand> validator,
      IAuthService auth,
      IHttpContextAccessor http)
    {
        _groupRepo = groupRepo;
        _userRepo = userRepo;
        _validator = validator;
        _auth = auth;
        _http = http;
    }

    public async Task<Result> Handle(
      RemoveUserFromGroupCommand request,
      CancellationToken ct)
    {
        var v = await _validator.ValidateAsync(request, ct);
        if (!v.IsValid)
            return Result.Invalid(v.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        var specGroup = new GroupByIdWithMembersSpec(request.GroupId);
        var group = await _groupRepo.FirstOrDefaultAsync(specGroup, ct);
        if (group is null) return Result.NotFound("Group not found.");

        var email = await _auth.GetEmailFromAuthTokenAsync(_http.HttpContext!, ct);
        var requesting = await _userRepo.FirstOrDefaultAsync(
          new KasaWGrupie.Persistence.Specifications.Users.UserByEmailSpecification(email),
          ct);
        if (requesting is null || requesting.Id != group.Admin.Id)
            return Result.Unauthorized();

        var member = group.Members.FirstOrDefault(u => u.Id == request.MemberId);
        if (member is null) return Result.NotFound("User not in group.");

        group.Members.Remove(member);
        await _groupRepo.UpdateAsync(group, ct);
        await _groupRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
