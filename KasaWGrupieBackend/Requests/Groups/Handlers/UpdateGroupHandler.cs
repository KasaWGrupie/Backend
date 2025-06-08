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


        await _groupRepository.UpdateAsync(group, cancellationToken);
        await _groupRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }


}