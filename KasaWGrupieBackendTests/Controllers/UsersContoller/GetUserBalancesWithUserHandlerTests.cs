// KasaWGrupie.Tests/GetUserBalancesWithUserHandlerTests.cs
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
    public class GetUserBalancesWithUserHandlerTests
    {
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<GetUserBalancesWithUserCommand>> _validator;
        private GetUserBalancesWithUserHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _userRepo = new Mock<IRepositoryBase<User>>();
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IValidator<GetUserBalancesWithUserCommand>>();

            _handler = new GetUserBalancesWithUserHandler(
                _userRepo.Object,
                _mediator.Object,
                _validator.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new GetUserBalancesWithUserCommand(0, 2);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult(new[]
               {
                   new ValidationFailure(nameof(cmd.UserId), "must be >0")
               }));

            var r = await _handler.Handle(cmd, CancellationToken.None);
            r.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenEitherUserMissing()
        {
            var cmd = new GetUserBalancesWithUserCommand(1, 2);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // first call: user
            _userRepo
              .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<User>>(),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync((User)null)   // user not found
              .ReturnsAsync(UserFactory.Create(id: 2)); // would-be other

            var r1 = await _handler.Handle(cmd, CancellationToken.None);
            r1.Status.Should().Be(ResultStatus.NotFound);

            // now user exists, other missing
            _userRepo
              .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<User>>(),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(UserFactory.Create(id: 1))
              .ReturnsAsync((User)null);

            var r2 = await _handler.Handle(cmd, CancellationToken.None);
            r2.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsEmpty_WhenNoCommonGroups()
        {
            var cmd = new GetUserBalancesWithUserCommand(1, 2);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            var u1 = UserFactory.Create(id: 1);
            var u2 = UserFactory.Create(id: 2);
            // leave u1.Groups and u2.Groups empty
            _userRepo
              .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<UserWithGroupsAndCurrencySpec>(),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(u1)
              .ReturnsAsync(u2);

            var r = await _handler.Handle(cmd, CancellationToken.None);
            r.Status.Should().Be(ResultStatus.Ok);
            var dto = r.Value!;
            dto.Balances.Should().BeEmpty();
            _mediator.Verify(m => m.Send(It.IsAny<GetGroupBalancesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_AggregatesCorrectly_ForCommonGroups()
        {
            var cmd = new GetUserBalancesWithUserCommand(1, 2);
            _validator
               .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());



            var u1 = UserFactory.Create(id: 1);

            var u2 = UserFactory.Create(id: 2);
            

            // User and Other share two groups
            var g1 = new Group { Id = 10, Admin = u1, Name = "G1", PictureUrl = "http://example.com/group.png", Description = "piwo", Currency = new Currency { Name = "USD" } };
            var g2 = new Group { Id = 20, Admin = u2, Name = "G2", PictureUrl = "http://example.com/group.png", Description = "piwo", Currency = new Currency { Name = "EUR" } };

            u1.Groups.Add(g1); u1.Groups.Add(g2);
            u2.Groups.Add(g1); u2.Groups.Add(g2);

            _userRepo
              .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<UserWithGroupsAndCurrencySpec>(),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(u1)
              .ReturnsAsync(u2);

            // Stub group balances:
            // For G1: 2→1=5, 1→2=3
            var balG1 = new GetGroupBalancesDto(10, new[]
            {
              new BalanceDto(2,1,5m),
              new BalanceDto(1,2,3m)
            });
            _mediator
              .SetupSequence(m => m.Send(
                It.Is<GetGroupBalancesCommand>(c => c.GroupId == 10),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result.Success(balG1));

            // For G2: only 2→1=7
            var balG2 = new GetGroupBalancesDto(20, new[]
            {
              new BalanceDto(2,1,7m)
            });
            _mediator
              .SetupSequence(m => m.Send(
                It.Is<GetGroupBalancesCommand>(c => c.GroupId == 20),
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result.Success(balG2));

            var r = await _handler.Handle(cmd, CancellationToken.None);
            r.Status.Should().Be(ResultStatus.Ok);

            var entries = r.Value!.Balances.ToList();
            entries.Should().HaveCount(2);

            // G1: net for user→other = 5-3 = 2 owed BY other?  rec=BalanceDto(2,1,5) AND (1,2,3)
            // Our logic uses the single rec if matched; here rec picks 2→1, so isOwed=true, amount=5
            entries.Should().ContainEquivalentOf(new UserToUserBalanceEntryDto(10, "G1", 5m, "USD", true));

            // G2: rec=2→1->7
            entries.Should().ContainEquivalentOf(new UserToUserBalanceEntryDto(20, "G2", 7m, "EUR", true));
        }
    }
}
