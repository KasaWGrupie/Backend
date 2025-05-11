using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.Currencies;
using KasaWGrupie.Persistence.Specifications.MoneyRequests;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Handlers;

public class CreateMoneyRequestHandler : IRequestHandler<CreateMoneyRequestCommand, Result>
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IRepositoryBase<PayRequest> _payRequestRepository;
    private readonly IRepositoryBase<Currency> _currencyRepository;
    private readonly IValidator<CreateMoneyRequestDto> _validator;

    public CreateMoneyRequestHandler(IRepositoryBase<User> userRepository, IRepositoryBase<Group> groupRepository, IRepositoryBase<PayRequest> payRequestRepository, IRepositoryBase<Currency> currencyRepository, IValidator<CreateMoneyRequestDto> validator)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _payRequestRepository = payRequestRepository;
        _currencyRepository = currencyRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(CreateMoneyRequestCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateMoneyRequestDto;
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }
        
        var sender = await _userRepository.GetByIdAsync(dto.SenderId, cancellationToken);
        if (sender == null)
        {
            return Result.NotFound("Sender user not found");
        }
        
        var receiver = await _userRepository.GetByIdAsync(dto.ReceiverId, cancellationToken);
        if (receiver == null)
        {
            return Result.NotFound("Receiver user not found");
        }
        
        var groups = new List<Group>();
        foreach (var groupId in dto.Groups)
        {
            var specification = new GetGroupByIdWithMembersSpecification(groupId);
            var group = await _groupRepository.FirstOrDefaultAsync(specification, cancellationToken);
            if (group == null)
            {
                return Result.NotFound("Group not found");
            }

            if (!group.Members.Contains(sender))
            {
                return Result.Invalid(new ValidationError("SenderId", $"Sender user is not a member of the group {groupId}"));
            }
            if (!group.Members.Contains(receiver))
            {
                return Result.Invalid(new ValidationError("ReceiverId", $"Receiver user is not a member of the group {groupId}"));
            }
            
            groups.Add(group);
        }
        
        var currencySpec = new CurrencyByNameSpecification(dto.Currency);
        var currency = await _currencyRepository.FirstOrDefaultAsync(currencySpec, cancellationToken);

        if (currency == null)
        {
            currency = new Currency { Name = dto.Currency };
            await _currencyRepository.AddAsync(currency, cancellationToken);
            await _currencyRepository.SaveChangesAsync(cancellationToken);
        }

        var payRequest = new PayRequest
        {
            Sender = sender,
            Receiver = receiver,
            GroupsToSettle = groups,
            //TODO amount
            Currency = currency,
            PayRequestStatus = PayRequestStatus.Pending
        };
        
        await _payRequestRepository.AddAsync(payRequest, cancellationToken);
        await _payRequestRepository.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}