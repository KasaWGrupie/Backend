using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Infrastructure.BalanceCalculator;                           // Real BalanceResult / BalanceRecord
using KasaWGrupie.Infrastructure.BalanceCalculator.HelperAdapters;
using KasaWGrupie.Persistence.Specifications.Groups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class GetGroupBalancesHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepo;
        private Mock<IRepositoryBase<Expense>> _expenseRepo;
        private Mock<IRepositoryBase<MoneyTransfer>> _moneyTransferRepo;
        private Mock<IGroupBalanceCalculator> _balanceCalculator;
        private Mock<IValidator<GetGroupBalancesCommand>> _validator;
        private GetGroupBalancesHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepo = new Mock<IRepositoryBase<Group>>();
            _expenseRepo = new Mock<IRepositoryBase<Expense>>();
            _moneyTransferRepo = new Mock<IRepositoryBase<MoneyTransfer>>();
            _balanceCalculator = new Mock<IGroupBalanceCalculator>();
            _validator = new Mock<IValidator<GetGroupBalancesCommand>>();

            _handler = new GetGroupBalancesHandler(
                _groupRepo.Object,
                _expenseRepo.Object,
                _moneyTransferRepo.Object,
                _balanceCalculator.Object,
                _validator.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            // Arrange
            var cmd = new GetGroupBalancesCommand(0);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(cmd.GroupId), "GroupId must be > 0")
                }));

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            var cmd = new GetGroupBalancesCommand(5);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _groupRepo
                .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithEmptyBalances_WhenNoData()
        {
            // Arrange
            var cmd = new GetGroupBalancesCommand(10);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Set up a fully initialized Group entity
            var admin = new User
            {
                Id = 1,
                Email = "admin@x.com",
                Name = "Admin",
                ProfilePictureUrl = "http://example.com/pic.png",
                Friends = new List<User>(),
                Groups = new List<Group>(),
                AdministratedGroups = new List<Group>(),
                SentPayRequests = new List<PayRequest>(),
                RecievedPayRequests = new List<PayRequest>(),
                SentFriendRequests = new List<FriendRequest>(),
                RecievedFriendRequests = new List<FriendRequest>()
            };
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "Test Group",
                Description = "A test group",
                PictureUrl = "http://example.com/group.png",
                Currency = new Currency { Name = "USD" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };

            _groupRepo
                .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _expenseRepo
                .Setup(r => r.ListAsync(
                    It.IsAny<ExpensesByGroupIdSpecification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Expense>());

            _moneyTransferRepo
                .Setup(r => r.ListAsync(
                    It.IsAny<MoneyTransfersByGroupIdSpecification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MoneyTransfer>());

            // Return a REAL, infrastructure-level BalanceResult (with an empty list)
            _balanceCalculator
                .Setup(b => b.CalculateBalanceInGroup(
                    It.IsAny<List<IExpenseBalance>>(),
                    It.IsAny<List<IMoneyTransferBalance>>()))
                .Returns(new KasaWGrupie.Infrastructure.BalanceCalculator.BalanceResult
                {
                    BalanceRecords = new List<KasaWGrupie.Infrastructure.BalanceCalculator.BalanceRecord>()
                }
                );

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Ok);
            var dto = result.Value;
            dto.GroupId.Should().Be(10);
            dto.Balances.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithCorrectBalances()
        {
            // Arrange
            var cmd = new GetGroupBalancesCommand(20);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var admin = new User
            {
                Id = 1,
                Email = "admin@x.com",
                Name = "Admin",
                ProfilePictureUrl = "http://example.com/pic.png",
                Friends = new List<User>(),
                Groups = new List<Group>(),
                AdministratedGroups = new List<Group>(),
                SentPayRequests = new List<PayRequest>(),
                RecievedPayRequests = new List<PayRequest>(),
                SentFriendRequests = new List<FriendRequest>(),
                RecievedFriendRequests = new List<FriendRequest>()
            };
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "Test Group",
                Description = "A test group",
                PictureUrl = "http://example.com/group.png",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };

            _groupRepo
                .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _expenseRepo
                .Setup(r => r.ListAsync(
                    It.IsAny<ExpensesByGroupIdSpecification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Expense>());

            _moneyTransferRepo
                .Setup(r => r.ListAsync(
                    It.IsAny<MoneyTransfersByGroupIdSpecification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MoneyTransfer>());

            
            // Prepare two real BalanceRecord instances
            var records = new List<KasaWGrupie.Infrastructure.BalanceCalculator.BalanceRecord>
            {
                new KasaWGrupie.Infrastructure.BalanceCalculator.BalanceRecord
                {
                    FromUserId = 2,
                    ToUserId = 1,
                    Amount = 50m
                },
                new KasaWGrupie.Infrastructure.BalanceCalculator.BalanceRecord
                {
                    FromUserId=3, 
                    ToUserId = 1,
                    Amount = 30m
                }
            };
            _balanceCalculator
                .Setup(b => b.CalculateBalanceInGroup(
                    It.IsAny<List<IExpenseBalance>>(),
                    It.IsAny<List<IMoneyTransferBalance>>()))
                .Returns(new KasaWGrupie.Infrastructure.BalanceCalculator.BalanceResult
                {
                    BalanceRecords = records
                });

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Ok);
            var dto = result.Value;
            dto.GroupId.Should().Be(20);
            dto.Balances.Count.Should().Be(2);

            dto.Balances.Should().Contain(b =>
                b.FromUserId == 2 && b.ToUserId == 1 && b.Amount == 50f);
            dto.Balances.Should().Contain(b =>
                b.FromUserId == 3 && b.ToUserId == 1 && b.Amount == 30f);
        }
    }
}
