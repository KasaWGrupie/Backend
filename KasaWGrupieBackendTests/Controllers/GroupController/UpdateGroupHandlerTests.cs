using Moq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.API.DTOs.Groups;
using FluentAssertions;
using KasaWGrupie.Infrastructure.ImageService;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Currencies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KasaWGrupie.Tests.Factories;
using Ardalis.Specification;
using FluentValidation;
using KasaWGrupie.API.Requests.Groups.Handlers;
using Microsoft.AspNetCore.Http;
using KasaWGrupie.Core.Enums;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class UpdateGroupHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
        private Mock<IRepositoryBase<User>> _userRepositoryMock;
        private Mock<IRepositoryBase<Currency>> _currencyRepositoryMock;
        private Mock<IImageService> _imageServiceMock;
        private Mock<IValidator<UpdateGroupCommand>> _validatorMock;
        private UpdateGroupHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
            _userRepositoryMock = new Mock<IRepositoryBase<User>>();
            _currencyRepositoryMock = new Mock<IRepositoryBase<Currency>>();
            _imageServiceMock = new Mock<IImageService>();
            _validatorMock = new Mock<IValidator<UpdateGroupCommand>>();

            _handler = new UpdateGroupHandler(
                _groupRepositoryMock.Object,
                _userRepositoryMock.Object,
                _currencyRepositoryMock.Object,
                _imageServiceMock.Object,
                _validatorMock.Object
            );
        }

        [TestMethod]
        public async Task Handle_ShouldUpdateGroup_WhenDataIsValid()
        {
            // Arrange
            var admin = UserFactory.Create(email: "admin@example.com");
            var group = new Group
            {
                Id = 1,
                Name = "Old Name",
                Description = "Old Description",
                PictureUrl = "old.jpg",
                Currency = new Currency { Name = "USD" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };

            var dto = new UpdateGroupDto(1, "New Name", "New Description", null,  null, null);
            var command = new UpdateGroupCommand(1, dto, null);
            var currency = new Currency { Name = "USD" };
            // Arrange: mock group fetch by ID
            _groupRepositoryMock
                .Setup(repo => repo.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            // Arrange: mock admin fetch
            _userRepositoryMock
                .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);


            // Arrange: mock currency fetch
            _currencyRepositoryMock
                .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<CurrencyByNameSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(currency);

            // Arrange: mock validator
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Arrange: mock image service
            _imageServiceMock
                .Setup(service => service.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UploadResult { IsSuccess = true, Url = "http://example.com/image.jpg" });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            group.Name.Should().Be("New Name");
            group.Description.Should().Be("New Description");
            _groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnNotFound_WhenGroupDoesNotExist()
        {
            var dto = new UpdateGroupDto(2, "Name", "Desc", null, null, null);
            var command = new UpdateGroupCommand(2, dto, null);

            _groupRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group?)null);

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

    
    }
}
