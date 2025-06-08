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

public class AddUserToGroupHandler
  : IRequestHandler<AddUserToGroupCommand, Result>
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IValidator<AddUserToGroupCommand> _validator;
    private readonly IAuthService _auth;
    private readonly IHttpContextAccessor _http;

    public AddUserToGroupHandler(
      IRepositoryBase<Group> groupRepo,
      IRepositoryBase<User> userRepo,
      IValidator<AddUserToGroupCommand> validator,
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
      AddUserToGroupCommand request,
      CancellationToken ct)
    {
        // 1) Validate
        var v = await _validator.ValidateAsync(request, ct);
        if (!v.IsValid)
            return Result.Invalid(v.Errors
              .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));

        // 2) Load group w/ members & admin
        var specGroup = new GroupByIdWithMembersSpec(request.GroupId);
        var group = await _groupRepo.FirstOrDefaultAsync(specGroup, ct);
        if (group is null) return Result.NotFound("Group not found.");

        // 3) Auth: only admin
        var email = await _auth.GetEmailFromAuthTokenAsync(_http.HttpContext!, ct);
        var requesting = await _userRepo.FirstOrDefaultAsync(
          new KasaWGrupie.Persistence.Specifications.Users.UserByEmailSpecification(email),
          ct);

        if (requesting is null || requesting.Id != group.Admin.Id)
            return Result.Unauthorized();

        // 4) Load member user
        var member = await _userRepo.GetByIdAsync(request.MemberId, ct);
        if (member is null) return Result.NotFound("User not found.");

        // 5) Add if not already
        if (!group.Members.Any(u => u.Id == member.Id))
            group.Members.Add(member);

        await _groupRepo.UpdateAsync(group, ct);
        await _groupRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
