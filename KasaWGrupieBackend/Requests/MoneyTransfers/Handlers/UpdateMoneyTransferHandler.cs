using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Handlers;

public class UpdateMoneyTransferHandler : IRequestHandler<UpdateMoneyTransferCommand, Result>
{
    private readonly IRepositoryBase<MoneyTransfer> _transferRepository;
    private readonly IValidator<UpdateMoneyTransferDto> _validator;

    public UpdateMoneyTransferHandler(IRepositoryBase<MoneyTransfer> transferRepository, IValidator<UpdateMoneyTransferDto> validator)
    {
        _transferRepository = transferRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UpdateMoneyTransferDto;
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }
        
        var transfer = await _transferRepository.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            return Result.NotFound("Money Transfer not found");
        }

        if (!Enum.TryParse<MoneyTransferStatus>(dto.Status, out var status))
        {
            return Result.Invalid(new ValidationError("Status", "Invalid status value"));
        }
        
        transfer.Status = status;

        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}