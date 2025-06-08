using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.MoneyTransfer;
using KasaWGrupie.API.Requests.MoneyTransfers.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.MoneyTransfers;
using MediatR;

namespace KasaWGrupie.API.Requests.MoneyTransfers.Handlers;

public class GetMoneyTransferForSenderHandler : IRequestHandler<GetMoneyTransferForSenderCommand, Result<List<GetMoneyTransferDto>>>
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<MoneyTransfer> _moneyTransferRepository;
    private readonly IValidator<GetMoneyTransferForSenderCommand> _validator;

    public GetMoneyTransferForSenderHandler(IRepositoryBase<User> userRepository, IRepositoryBase<MoneyTransfer> moneyTransferRepository, IValidator<GetMoneyTransferForSenderCommand> validator)
    {
        _userRepository = userRepository;
        _moneyTransferRepository = moneyTransferRepository;
        _validator = validator;
    }

    public async Task<Result<List<GetMoneyTransferDto>>> Handle(GetMoneyTransferForSenderCommand request, CancellationToken cancellationToken)
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

        ISpecification<MoneyTransfer> specification;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<MoneyTransferStatus>(request.Status, ignoreCase: true, out var status))
            specification = new GetMoneyTransfersBySenderWithStatusSpecification(request.SenderId, status);
        else
            specification = new GetMoneyTransfersBySenderSpecification(request.SenderId);

        var requests = await _moneyTransferRepository.ListAsync(specification, cancellationToken);

        var dtos = requests.Select(pr => new GetMoneyTransferDto(
            pr.Id,
            pr.SenderId,
            pr.RecipientId,
            pr.Amount,
            pr.GroupId,
            pr.EndDate,
            pr.Status.ToString()
        )).ToList();

        return Result.Success(dtos);
    }
}