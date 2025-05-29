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
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Tests.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class GetGroupBalancesHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepoMock;
        private Mock<IRepositoryBase<Expense>> _expenseRepoMock;
        private Mock<IValidator<GetGroupBalancesCommand>> _validatorMock;
        private GetGroupBalancesHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepoMock = new Mock<IRepositoryBase<Group>>();
            _expenseRepoMock = new Mock<IRepositoryBase<Expense>>();
            _validatorMock = new Mock<IValidator<GetGroupBalancesCommand>>();

            _handler = new GetGroupBalancesHandler(
              _groupRepoMock.Object,
              _expenseRepoMock.Object,
              _validatorMock.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new GetGroupBalancesCommand(-1);
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(cmd.GroupId), "must be positive")
            };
            _validatorMock
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult(failures));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var groupId = 99;
            var cmd = new GetGroupBalancesCommand(groupId);

            _validatorMock
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            _groupRepoMock
              .Setup(r => r.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync((Group)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithAggregatedBalances()
        {
            // Arrange
            var groupId = 1;
            var cmd = new GetGroupBalancesCommand(groupId);

            _validatorMock
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            // Users
            var admin = UserFactory.Create(email: "admin@x.com", id: 1);
            var u1 = UserFactory.Create(email: "u1@x.com", id: 2);
            var u2 = UserFactory.Create(email: "u2@x.com", id: 3);

            // Group
            var group = new Group
            {
                Id = groupId,
                Name = "Test Group",
                Description = "Desc",
                PictureUrl = "pic.jpg",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin, u1, u2 },
                Status = GroupStatus.Active
            };
            _groupRepoMock
              .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            // Expense #1 with its split and two split-records
            var expense1 = new Expense
            {
                Id = 10,
                Group = group,
                GroupId = groupId,
                PayingPerson = admin,
                PayingPersonId = admin.Id,
                Name = "E1",
                Description = "first",
                PictureUrl = "p1.jpg",
                Amount = 80m,                        // total (not used)
                Date = System.DateTime.UtcNow,
                ExpenseSplit = new ExpenseSplit
                {
                    // circular back-pointer set here
                    Expense = null!,                  // placeholder
                    ExpenseId = 10,
                    Type = ExpenseSplitType.Equally,
                    SplitRecords = new List<ExpenseSplitRecord>
            {
                new ExpenseSplitRecord
                {
                    ExpenseSplit   = null!,         // placeholder
                    OwingPerson    = u1,
                    OwingPersonId  = u1.Id,
                    Amount         = 50m,
                    Percentage     = 0m
                },
                new ExpenseSplitRecord
                {
                    ExpenseSplit   = null!,         // placeholder
                    OwingPerson    = u2,
                    OwingPersonId  = u2.Id,
                    Amount         = 30m,
                    Percentage     = 0m
                }
            }
                }
            };
            // now patch placeholders
            expense1.ExpenseSplit.Expense = expense1;
            foreach (var rec in expense1.ExpenseSplit.SplitRecords)
                rec.ExpenseSplit = expense1.ExpenseSplit;

            // Expense #2 with its split and one record
            var expense2 = new Expense
            {
                Id = 11,
                Group = group,
                GroupId = groupId,
                PayingPerson = admin,
                PayingPersonId = admin.Id,
                Name = "E2",
                Description = "second",
                PictureUrl = "p2.jpg",
                Amount = 20m,
                Date = System.DateTime.UtcNow,
                ExpenseSplit = new ExpenseSplit
                {
                    Expense = null!,
                    ExpenseId = 11,
                    Type = ExpenseSplitType.Equally,
                    SplitRecords = new List<ExpenseSplitRecord>
            {
                new ExpenseSplitRecord
                {
                    ExpenseSplit   = null!,
                    OwingPerson    = u1,
                    OwingPersonId  = u1.Id,
                    Amount         = 20m,
                    Percentage     = 0m
                }
            }
                }
            };
            expense2.ExpenseSplit.Expense = expense2;
            foreach (var rec in expense2.ExpenseSplit.SplitRecords)
                rec.ExpenseSplit = expense2.ExpenseSplit;

            _expenseRepoMock
              .Setup(x => x.ListAsync(
                  It.IsAny<ISpecification<Expense>>(),
                  It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<Expense> { expense1, expense2 });

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Ok);
            var balances = result.Value!.Balances.ToList();

            balances.Should().HaveCount(2);
            balances.Should().Contain(b =>
                b.FromUserId == u1.Id &&
                b.ToUserId == admin.Id &&
                b.Amount == 70f   // 50 + 20
            );
            balances.Should().Contain(b =>
                b.FromUserId == u2.Id &&
                b.ToUserId == admin.Id &&
                b.Amount == 30f
            );
        }


    }
}
