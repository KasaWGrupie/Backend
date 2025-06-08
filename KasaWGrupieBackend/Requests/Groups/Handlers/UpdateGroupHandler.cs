using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.Requests.Groups.Commands;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.Infrastructure.ImageService;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Currencies;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class UpdateGroupHandler : IRequestHandler<UpdateGroupCommand, Result>
{
    private readonly IRepositoryBase<Group> _groupRepository;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<Currency> _currencyRepository;
    private readonly IImageService _imageService;
    private readonly IValidator<UpdateGroupCommand> _validator;
    public UpdateGroupHandler(IRepositoryBase<Group> groupRepository, IRepositoryBase<User> userRepository, IRepositoryBase<Currency> currencyRepository, IImageService imageService, IValidator<UpdateGroupCommand> validator)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _imageService = imageService;
        _validator = validator;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UpdateGroupDto;

        // Optional: Adjust validation if validator assumes all fields are required.
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }

        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.NotFound();

        if (request.UserId != group.AdminId)
        {
            return Result.Forbidden("Only admin can update the group.");
        }
        
        // Admin
        User? admin = null;
        if (!string.IsNullOrWhiteSpace(dto.AdminEmail))
        {
            admin = await _userRepository.FirstOrDefaultAsync(new UserByEmailSpecification(dto.AdminEmail), cancellationToken);
            if (admin == null)
                return Result.Invalid(new ValidationError("AdminEmail", "Admin user does not exist."));
        }

        // Members
        List<User>? members = null;
        if (dto.Members != null)
        {
            members = new List<User>();
            foreach (var email in dto.Members)
            {
                var member = await _userRepository.FirstOrDefaultAsync(new UserByEmailSpecification(email), cancellationToken);
                if (member == null)
                    return Result.Invalid(new ValidationError("Members", $"User with email {email} does not exist."));
                members.Add(member);
            }
        }

        // Currency
        Currency? currency = null;
        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            currency = await _currencyRepository.FirstOrDefaultAsync(new CurrencyByNameSpecification(dto.Currency), cancellationToken);
            if (currency == null)
            {
                currency = new Currency { Name = dto.Currency };
                await _currencyRepository.AddAsync(currency, cancellationToken);
                await _currencyRepository.SaveChangesAsync(cancellationToken);
            }
        }

        // Image
        string? imageUrl = null;
        if (request.Image != null)
        {
            var uploadResult = await _imageService.UploadImageAsync(request.Image, cancellationToken);
            if (uploadResult.IsSuccess)
                imageUrl = uploadResult.Url;
        }

        // Final update
        if (dto.Name != null)
            group.Name = dto.Name;

        if (dto.Description != null)
            group.Description = dto.Description;

        if (imageUrl != null)
            group.PictureUrl = imageUrl;

        if (admin != null)
            group.Admin = admin;

        if (members != null)
            group.Members = members;

        if (currency != null)
            group.Currency = currency;

        await _groupRepository.UpdateAsync(group, cancellationToken);
        await _groupRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }


}