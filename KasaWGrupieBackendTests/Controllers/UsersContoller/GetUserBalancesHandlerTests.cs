using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.DTOs.Groups;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Tests.Factories;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class GetUserBalancesHandlerTests
    {
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<GetUserBalancesCommand>> _validator;
        private GetUserBalancesHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _userRepo = new Mock<IRepositoryBase<User>>();
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IValidator<GetUserBalancesCommand>>();

            _handler = new GetUserBalancesHandler(
                _userRepo.Object,
                _mediator.Object,
                _validator.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new GetUserBalancesCommand(0);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult(new[]
               {
                   new ValidationFailure(nameof(cmd.UserId), "must be > 0")
               }));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var cmd = new GetUserBalancesCommand(42);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            _userRepo
               .Setup(r => r.FirstOrDefaultAsync(
                   It.IsAny<ISpecification<User>>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync((User)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithEmptyLists_WhenUserHasNoGroups()
        {
            var userId = 7;
            var cmd = new GetUserBalancesCommand(userId);

            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // User with no groups
            var user = UserFactory.Create(id: userId);
            user.Groups.Clear();

            _userRepo
               .Setup(r => r.FirstOrDefaultAsync(
                   It.IsAny<UserWithGroupsSpec>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Ok);
            var dto = result.Value!;
            dto.UserId.Should().Be(userId);
            dto.OwedByOthers.Should().BeEmpty();
            dto.OwesToOthers.Should().BeEmpty();

            // Mediator should never be called
            _mediator.Verify(m => m.Send(It.IsAny<GetGroupBalancesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithAggregatedBalances()
        {
            var userId = 100;
            var cmd = new GetUserBalancesCommand(userId);

            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // Create two groups and attach to user
            var user = UserFactory.Create(id: userId);
            var group1 = new Group
            {
                Id = 1,
                Name = "G1",
                Description = "D1",
                PictureUrl = "P1",
                Currency = new Currency { Name = "USD" },
                Admin = user,
                Members = new List<User> { user },
                Status = GroupStatus.Active
            };
            var group2 = new Group
            {
                Id = 2,
                Name = "G2",
                Description = "D2",
                PictureUrl = "P2",
                Currency = new Currency { Name = "EUR" },
                Admin = user,
                Members = new List<User> { user },
                Status = GroupStatus.Active
            };
            user.Groups.Clear();
            user.Groups.Add(group1);
            user.Groups.Add(group2);

            _userRepo
               .Setup(r => r.FirstOrDefaultAsync(
                   It.IsAny<UserWithGroupsSpec>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

            // Stub mediator for group1: user is owed 10 by user 5, user owes 7 to user 6
            var bal1 = new GetGroupBalancesDto(1, new[]
            {
                new BalanceDto(5, userId, 10m),
                new BalanceDto(userId, 6, 7m)
            });
            _mediator
               .Setup(m => m.Send(
                   It.Is<GetGroupBalancesCommand>(c => c.GroupId == 1),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result.Success(bal1));

            // Stub mediator for group2: user is owed 3 by user 5, user owes 4 to user 6
            var bal2 = new GetGroupBalancesDto(2, new[]
            {
                new BalanceDto(5, userId, 3m),
                new BalanceDto(userId, 6, 4m)
            });
            _mediator
               .Setup(m => m.Send(
                   It.Is<GetGroupBalancesCommand>(c => c.GroupId == 2),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result.Success(bal2));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Ok);
            var dto = result.Value!;

            dto.UserId.Should().Be(userId);

            // OwedByOthers: user 5 owes 10 + 3 = 13
            dto.OwedByOthers.Should().HaveCount(1)
               .And.ContainEquivalentOf(new UserBalanceEntryDto(5, 13m));

            // OwesToOthers: owes user 6 => 7 + 4 = 11
            dto.OwesToOthers.Should().HaveCount(1)
               .And.ContainEquivalentOf(new UserBalanceEntryDto(6, 11m));
        }
    }
}
