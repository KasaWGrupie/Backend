using Ardalis.Result;
using KasaWGrupie.API.DTOs.Users;
using MediatR;

namespace KasaWGrupie.API.Requests.Users.Commands
{
    public record GetUserGroupsCommand(string UserEmail) : IRequest<Result<List<GetUserGroupsDto>>>;

}
