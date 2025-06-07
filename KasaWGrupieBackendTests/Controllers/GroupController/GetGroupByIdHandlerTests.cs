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
    public class GetGroupByIdHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepoMock;
        private Mock<IValidator<GetGroupByIdCommand>> _validatorMock;
        private GetGroupByIdHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepoMock = new Mock<IRepositoryBase<Group>>();
            _validatorMock = new Mock<IValidator<GetGroupByIdCommand>>();

            _handler = new GetGroupByIdHandler(
                _groupRepoMock.Object,
                _validatorMock.Object
            );
        }

        [TestMethod]
        public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
        {
            // Arrange
            var cmd = new GetGroupByIdCommand(0);
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(cmd.GroupId), "must be greater than zero")
            };
            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            var cmd = new GetGroupByIdCommand(123);
            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _groupRepoMock
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnSuccess_WithCorrectDto()
        {
            // Arrange
            var groupId = 7;
            var cmd = new GetGroupByIdCommand(groupId);

            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Create users
            var admin = UserFactory.Create(id: 10, email: "admin@x.com");
            var m1 = UserFactory.Create(id: 20, email: "m1@x.com");
            var m2 = UserFactory.Create(id: 30, email: "m2@x.com");

            // Build group entity
            var group = new Group
            {
                Id = groupId,
                Name = "My Group",
                Description = "Test Desc",
                PictureUrl = "http://img.jpg",
                Currency = new Currency { Name = "PLN" },
                Admin = admin,
                Members = new List<User> { admin, m1, m2 },
                Status = GroupStatus.Active
            };

            _groupRepoMock
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Ok);

            var dto = result.Value!;
            dto.Should().BeOfType<GroupDto>();
            dto.Id.Should().Be(groupId);
            dto.Name.Should().Be(group.Name);
            dto.Description.Should().Be(group.Description);
            dto.PictureUrl.Should().Be(group.PictureUrl);
            dto.Currency.Should().Be(group.Currency.Name);
            dto.AdminId.Should().Be(admin.Id);
            dto.Status.Should().Be(group.Status.ToString());
            dto.Members.Should().BeEquivalentTo(new[] { admin.Id, m1.Id, m2.Id });
        }
    }
}
