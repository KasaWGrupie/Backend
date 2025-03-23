using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.ImageService;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KasaWGrupieTests;

[TestClass]
public class CreateUserHandlerTests
{
	
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private Mock<IRepositoryBase<User>> _userRepositoryMock;
		private Mock<IImageService> _imageServiceMock;
		private Mock<IValidator<CreateUserDto>> _validatorMock;
		private CreateUserHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IRepositoryBase<User>>();
			_imageServiceMock = new Mock<IImageService>();
			_validatorMock = new Mock<IValidator<CreateUserDto>>();

			_handler = new CreateUserHandler(
				_userRepositoryMock.Object,
				_imageServiceMock.Object,
				_validatorMock.Object
			);
		}

		[TestMethod]
		public async Task Handle_ShouldReturnSuccess_WhenUserIsCreatedSuccessfully()
		{
			// Arrange
			var createGroupDto = new CreateUserDto(
				"User1",
				"user1@example.com",
				null
			);

			var command = new CreateUserCommand(createGroupDto);
			
			// Mockujemy odpowiedzi
			_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new FluentValidation.Results.ValidationResult());

			_imageServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new UploadResult { IsSuccess = true, Url = "http://example.com/image.jpg" });

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			result.IsSuccess.Should().BeTrue();
			_userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
		}

	}
