using Ardalis.Specification;
using MediatR;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.DTOs.Groups;
using Ardalis.Result;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class GetGroupInviteCodeHandler : IRequestHandler<GetGroupInviteCodeCommand, Result<InviteCodeDto>>
{
	private readonly IRepositoryBase<Group> _groupRepository;
	private readonly IValidator<GetGroupInviteCodeCommand> _validator;

	public GetGroupInviteCodeHandler(IRepositoryBase<Group> groupRepository, IValidator<GetGroupInviteCodeCommand> validator)
	{
		_groupRepository = groupRepository;
		_validator = validator;
	}
	public async Task<Result<InviteCodeDto>> Handle(GetGroupInviteCodeCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Result<InviteCodeDto>.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);

		if (group == null)
		{
			return Result<InviteCodeDto>.NotFound();
		}

		if (string.IsNullOrEmpty(group.InviteCode))
		{
			group.InviteCode = Guid.NewGuid().ToString("N").Substring(0, 12);
			await _groupRepository.UpdateAsync(group, cancellationToken);
			await _groupRepository.SaveChangesAsync(cancellationToken);
		}

		return Result<InviteCodeDto>.Success(new InviteCodeDto(group.InviteCode));
	}
}
