using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Users;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Handlers;

public class SearchUsersByPartialEmailHandler : IRequestHandler<SearchUsersByPartialEmailCommand, Result<List<GetUserDto>>>
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IValidator<SearchUsersByPartialEmailCommand> _validator;

    public SearchUsersByPartialEmailHandler(IRepositoryBase<User> userRepository, IValidator<SearchUsersByPartialEmailCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<List<GetUserDto>>> Handle(SearchUsersByPartialEmailCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Invalid(validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
        }
        
        var specification = new GetUsersLikeEmailSpecification(request.Email);
        var friends = await _userRepository.ListAsync(specification, cancellationToken);
        
        var dtos = friends.Select(u => new GetUserDto {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            ProfilePictureUrl = u.ProfilePictureUrl
        }).ToList();
        
        return Result.Success(dtos);
    }   
}