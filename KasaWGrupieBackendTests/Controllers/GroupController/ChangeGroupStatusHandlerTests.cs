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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class ChangeGroupStatusHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepoMock;
        private Mock<IValidator<ChangeGroupStatusCommand>> _validatorMock;
        private ChangeGroupStatusHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepoMock = new Mock<IRepositoryBase<Group>>();
            _validatorMock = new Mock<IValidator<ChangeGroupStatusCommand>>();

            _handler = new ChangeGroupStatusHandler(
                _groupRepoMock.Object,
                _validatorMock.Object
            );
        }

        [TestMethod]
        public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
        {
            // Arrange
            var dto = new ChangeGroupStatusDto(0, GroupStatus.Closed);
            var cmd = new ChangeGroupStatusCommand(dto);
            var failures = new[] { new ValidationFailure("GroupId", "must be > 0") };
            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnNotFound_WhenGroupNotExists()
        {
            // Arrange
            var dto = new ChangeGroupStatusDto(123, GroupStatus.Closed);
            var cmd = new ChangeGroupStatusCommand(dto);
            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _groupRepoMock
                .Setup(r => r.GetByIdAsync(dto.GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ShouldUpdateStatusAndReturnSuccess_WhenGroupExists()
        {
            // Arrange
            var dto = new ChangeGroupStatusDto(5, GroupStatus.Closed);
            var cmd = new ChangeGroupStatusCommand(dto);
            _validatorMock
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var group = new Group
            {
                Id = dto.GroupId,
                Name = "G",
                Description = "D",
                PictureUrl = "P",
                Currency = new Currency { Name = "EUR" },
                Admin = null!,
                Members = null!,
                Status = GroupStatus.Active
            };

            _groupRepoMock
                .Setup(r => r.GetByIdAsync(dto.GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Status.Should().Be(ResultStatus.Ok);
            group.Status.Should().Be(GroupStatus.Closed);

            _groupRepoMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
            _groupRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
