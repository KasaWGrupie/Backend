using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.MoneyRequests;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Handlers;

public class CreateMoneyTransferHandler : IRequestHandler<CreateMoneyTransferCommand, Result>
{
    private readonly IRepositoryBase<MoneyTransfer> _transferRepository;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IValidator<CreateMoneyTransferDto> _validator;

    public CreateMoneyTransferHandler(IRepositoryBase<MoneyTransfer> transferRepository, IRepositoryBase<User> userRepository, IRepositoryBase<Group> groupRepository, IValidator<CreateMoneyTransferDto> validator)
    {
        _transferRepository = transferRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(CreateMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateMoneyTransferDto;
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }

        if (request.UserId != dto.SenderId)
        {
            return Result.Forbidden("You are not allowed to create money transfers for other users.");
        }
        
        var sender = await _userRepository.GetByIdAsync(dto.SenderId, cancellationToken);
        if (sender == null)
        {
            return Result.Invalid(new ValidationError("SenderId", "Sender user not found"));
        }
        
        var recipient = await _userRepository.GetByIdAsync(dto.RecipientId, cancellationToken);
        if (recipient == null)
        {
            return Result.Invalid(new ValidationError("RecipientId", "Recipient user not found"));
        }

        var specification = new GetGroupByIdWithMembersSpecification(dto.GroupId);
        var group = await _groupRepository.FirstOrDefaultAsync(specification, cancellationToken);
        if (group == null)
        {
            return Result.Invalid(new ValidationError("GroupId", "Group not found"));
        }

        if (!group.Members.Contains(sender))
        {
            return Result.Invalid(new ValidationError("SenderId", "Sender user is not a member of the group"));
        }
        if (!group.Members.Contains(recipient))
        {
            return Result.Invalid(new ValidationError("RecipientId", "Recipient user is not a member of the group"));
        }

        var moneyTransfer = new MoneyTransfer
        {
            Recipient = recipient,
            Sender = sender,
            Amount = dto.Amount,
            Group = group,
            Status = MoneyTransferStatus.Pending
        };
        
        await _transferRepository.AddAsync(moneyTransfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}