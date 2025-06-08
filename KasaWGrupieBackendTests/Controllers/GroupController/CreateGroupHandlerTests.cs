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

namespace KasaWGrupie.Tests
{
	[TestClass]
	public class CreateGroupHandlerTests
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private Mock<IRepositoryBase<Group>> _groupRepositoryMock;
		private Mock<IRepositoryBase<User>> _userRepositoryMock;
		private Mock<IRepositoryBase<Currency>> _currencyRepositoryMock;
		private Mock<IImageService> _imageServiceMock;
		private Mock<IValidator<CreateGroupCommand>> _validatorMock;
		private CreateGroupHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		[TestInitialize]
		public void Setup()
		{
			_groupRepositoryMock = new Mock<IRepositoryBase<Group>>();
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_currencyRepositoryMock = new Mock<IRepositoryBase<Currency>>();
			_imageServiceMock = new Mock<IImageService>();
			_validatorMock = new Mock<IValidator<CreateGroupCommand>>();

			_handler = new CreateGroupHandler(
				_groupRepositoryMock.Object,
				_userRepositoryMock.Object,
				_currencyRepositoryMock.Object,
				_imageServiceMock.Object,
				_validatorMock.Object
			);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnSuccess_WhenGroupIsCreatedSuccessfully()
		{
			// Arrange
			var createGroupDto = new CreateGroupDto(
				"Group1",
				"Description",
				"USD",
				1,
				new List<int> { 1, 2 }
			);


			var admin = UserFactory.Create(email: "admin@example.com", id: 1); // Tworzymy admina
			var user1 = UserFactory.Create(email: "user1@example.com", id: 2); // Tworzymy członka 1

			var command = new CreateGroupCommand(admin.Id, createGroupDto, null);

			var currency = new Currency
			{
				Id = 1,
				Name = "USD"
			};

			// Mockujemy odpowiedzi repozytoriów
			_userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
				.ReturnsAsync(admin);

			_userRepositoryMock.Setup(repo => repo.GetByIdAsync(2, It.IsAny<CancellationToken>()))
				.ReturnsAsync(admin);

			_currencyRepositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<CurrencyByNameSpecification>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(currency);

			_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new FluentValidation.Results.ValidationResult());

			_imageServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new UploadResult { IsSuccess = true, Url = "http://example.com/image.jpg" });

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeTrue();
			_groupRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnInvalid_WhenAdminUserDoesNotExist()
		{
			// Arrange
			var createGroupDto = new CreateGroupDto(
				"Group1",
				"Description",
				"USD",
				1,
				new List<int> { 1 }
			);

			var command = new CreateGroupCommand(1, createGroupDto, null);

			_userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
				.ReturnsAsync((User?)null); // Admin user does not exist

			_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new FluentValidation.Results.ValidationResult());

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeFalse();
		}

		[TestMethod]
		public async Task Handle_ShouldReturnInvalid_WhenMemberDoesNotExist()
		{
			// Arrange
			var createGroupDto = new CreateGroupDto(
				"Group1",
				"Description",
				"USD",
				1,
				new List<int> { 1 }
			);

			var admin = UserFactory.Create(email: "admin@example.com");

			var command = new CreateGroupCommand(1, createGroupDto, null);

			_userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
				.ReturnsAsync(admin);

			_userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
				.ReturnsAsync((User?)null); // Member does not exist

			_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new FluentValidation.Results.ValidationResult());

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeFalse();
		}


		[TestMethod]
		public async Task Handle_ShouldReturnInvalid_WhenAdminEmailsDoNotMatch()
		{
			// Arrange
			var createGroupDto = new CreateGroupDto(
				"Group1",
				"Description",
				"USD",
				1,
				new List<int> { 1 }
			);

			var command = new CreateGroupCommand(2, createGroupDto, null);

			_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new FluentValidation.Results.ValidationResult());

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeFalse();
		}

	}
}
