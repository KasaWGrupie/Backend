using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.MoneyRequests;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Handlers;

public class GetMoneyRequestForSenderHandler : IRequestHandler<GetMoneyRequestForSenderCommand, Result<List<GetMoneyRequestDto>>>
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<PayRequest> _payRequestRepository;
    private readonly IValidator<GetMoneyRequestForSenderCommand> _validator;

    public GetMoneyRequestForSenderHandler(IRepositoryBase<User> userRepository, IRepositoryBase<PayRequest> payRequestRepository, IValidator<GetMoneyRequestForSenderCommand> validator)
    {
        _userRepository = userRepository;
        _payRequestRepository = payRequestRepository;
        _validator = validator;
    }

    public async Task<Result<List<GetMoneyRequestDto>>> Handle(GetMoneyRequestForSenderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }
        
        var sender = await _userRepository.GetByIdAsync(request.SenderId, cancellationToken);
        if (sender == null)
        {
            return Result.NotFound("Sender user not found");
        }
        
        ISpecification<PayRequest> specification;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<PayRequestStatus>(request.Status, ignoreCase: true, out var status))
            specification = new GetMoneyRequestsBySenderWithStatusSpecification(request.SenderId, status);
        else
            specification = new GetMoneyRequestsBySenderSpecification(request.SenderId);
        
        var requests = await _payRequestRepository.ListAsync(specification, cancellationToken);
        
        var dtos = requests.Select(pr => new GetMoneyRequestDto(
            pr.Id,
            pr.SenderId,
            pr.ReceiverId,
            pr.Amount,
            pr.Currency.Name,
            pr.GroupsToSettle.Select(g => g.Id).ToList(),
            pr.PayRequestStatus.ToString(),
            pr.EndDate
        )).ToList();
        
        return Result.Success(dtos);
    }
}