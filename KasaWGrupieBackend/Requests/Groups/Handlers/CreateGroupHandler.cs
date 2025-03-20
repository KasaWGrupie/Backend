using MediatR;
using Ardalis.Result;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Enums;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.Infrastructure.ImageService;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Currencies;

namespace KasaWGrupie.API.Requests.Groups.Handlers;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, Result>
{
	private readonly IRepositoryBase<Group> _groupRepository;
	private readonly IRepositoryBase<User> _userRepository;
	private readonly IRepositoryBase<Currency> _currencyRepository;
	private readonly IImageService _imageService;
	private readonly IValidator<CreateGroupDto> _validator;
	public CreateGroupHandler(IRepositoryBase<Group> groupRepository, IRepositoryBase<User> userRepository, IRepositoryBase<Currency> currencyRepository, IImageService imageService, IValidator<CreateGroupDto> validator)
	{
		_groupRepository = groupRepository;
		_userRepository = userRepository;
		_imageService = imageService;
		_validator = validator;
		_currencyRepository = currencyRepository;
	}
	public async Task<Result> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request.CreateGroupDto, cancellationToken);

		if (!validationResult.IsValid)
		{
			return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
		}

		var members = new List<User>();

		foreach (var memberEmail in request.CreateGroupDto.Members)
		{
			var userSpecification = new UserByEmailSpecification(memberEmail);
			var member = await _userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);
			if (member == null)
			{
				return Result.Invalid(new ValidationError("Members", $"User with email {memberEmail} does not exist."));
			}

			members.Add(member);
		}

		var adminSpec = new UserByEmailSpecification(request.CreateGroupDto.AdminEmail);
		var admin = await _userRepository.FirstOrDefaultAsync(adminSpec, cancellationToken);

		if (admin == null)
		{
			return Result.Invalid(new ValidationError("AdminEmail", "Admin user does not exist."));
		}

		var imageUrl = string.Empty;
		if (request.CreateGroupDto.Image != null)
		{
			var uploadResult = await _imageService.UploadImageAsync(request.CreateGroupDto.Image, cancellationToken);
			if (uploadResult.IsSuccess)
			{
				imageUrl = uploadResult.Url;
			}
		}

		var currencySpec = new CurrencyByNameSpecification(request.CreateGroupDto.Currency);
		var currency = await _currencyRepository.FirstOrDefaultAsync(currencySpec, cancellationToken);

		if (currency == null)
		{
			currency = new Currency { Name = request.CreateGroupDto.Currency };
			await _currencyRepository.AddAsync(currency, cancellationToken);
			await _currencyRepository.SaveChangesAsync(cancellationToken);
		}

		var group = new Group
		{
			Name = request.CreateGroupDto.Name,
			PictureUrl = imageUrl!,
			Description = request.CreateGroupDto.Description,
			Currency = currency,
			Admin = admin,
			Members = members,
			Status = GroupStatus.Active
		};

		await _groupRepository.AddAsync(group, cancellationToken);
		await _groupRepository.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}