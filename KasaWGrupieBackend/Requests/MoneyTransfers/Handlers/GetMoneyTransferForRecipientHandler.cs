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

public class GetMoneyTransferForRecipientHandler : IRequestHandler<GetMoneyTransferForRecipientCommand, Result<List<GetMoneyTransferDto>>>
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<MoneyTransfer> _moneyTransferRepository;
    private readonly IValidator<GetMoneyTransferForRecipientCommand> _validator;

    public GetMoneyTransferForRecipientHandler(IRepositoryBase<User> userRepository, IRepositoryBase<MoneyTransfer> moneyTransferRepository, IValidator<GetMoneyTransferForRecipientCommand> validator)
    {
        _userRepository = userRepository;
        _moneyTransferRepository = moneyTransferRepository;
        _validator = validator;
    }

    public async Task<Result<List<GetMoneyTransferDto>>> Handle(GetMoneyTransferForRecipientCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }

        var recipient = await _userRepository.GetByIdAsync(request.RecipientId, cancellationToken);
        if (recipient == null)
        {
            return Result.NotFound("Recipient user not found");
        }

        ISpecification<MoneyTransfer> specification;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<MoneyTransferStatus>(request.Status, ignoreCase: true, out var status))
            specification = new GetMoneyTransfersByRecipientWithStatusSpecification(request.RecipientId, status);
        else
            specification = new GetMoneyTransfersByRecipientSpecification(request.RecipientId);

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