using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyRequest;
using KasaWGrupie.API.Requests.MoneyRequest.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyRequest.Handlers;

public class UpdateMoneyRequestStatusHandler : IRequestHandler<UpdateMoneyRequestStatusCommand, Result>
{
    private readonly IRepositoryBase<PayRequest> _payRequestRepository;
    private readonly IValidator<UpdateMoneyRequestStatusDto> _validator;

    public UpdateMoneyRequestStatusHandler(IRepositoryBase<PayRequest> payRequestRepository, IValidator<UpdateMoneyRequestStatusDto> validator)
    {
        _payRequestRepository = payRequestRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateMoneyRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UpdateMoneyRequestStatusDto;
        
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }
        
        var payRequest = await _payRequestRepository.GetByIdAsync(dto.RequestId, cancellationToken);
        if (payRequest == null)
        {
            return Result.NotFound("Pay request not found");
        }

        if (!Enum.TryParse<PayRequestStatus>(dto.Status, out var status))
        {
            return Result.Invalid(new ValidationError("Status", "Invalid status value"));
        }
        
        payRequest.PayRequestStatus = status;
        if (status != PayRequestStatus.Pending)
        {
            payRequest.EndDate = DateTime.Now;
        }
        
        await _payRequestRepository.UpdateAsync(payRequest, cancellationToken);
        await _payRequestRepository.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}