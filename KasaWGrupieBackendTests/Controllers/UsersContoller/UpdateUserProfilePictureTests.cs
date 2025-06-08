using Ardalis.Result;
using Ardalis.Specification;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.DTOs.Users;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Infrastructure.ImageService;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KasaWGrupie.Tests.Controllers.UserControllers;

[TestClass]
public class UpdateUserProfilePictureHandlerTests
{
	private Mock<IRepositoryBase<User>> _userRepositoryMock;
	private Mock<IImageService> _imageServiceMock;
	private Mock<IValidator<UpdateUserProfilePictureCommand>> _validatorMock;
	private UpdateUserProfilePictureHandler _handler;

	[TestInitialize]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IRepositoryBase<User>>();
		_imageServiceMock = new Mock<IImageService>();
		_validatorMock = new Mock<IValidator<UpdateUserProfilePictureCommand>>();

		_handler = new UpdateUserProfilePictureHandler(
			_userRepositoryMock.Object,
			_imageServiceMock.Object,
			_validatorMock.Object
		);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnInvalid_WhenValidationFails()
	{
		// Arrange
		var command = new UpdateUserProfilePictureCommand(1, null); // Null image
		var validationFailures = new List<FluentValidation.Results.ValidationFailure>
		{
			new FluentValidation.Results.ValidationFailure("ProfilePicture", "Profile picture is required.")
		};
		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserProfilePictureCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsInvalid());
	}

	[TestMethod]
	public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
	{
		// Arrange
		var command = new UpdateUserProfilePictureCommand(1, new Mock<IFormFile>().Object);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserProfilePictureCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((User)null!); // No user found

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsNotFound());
	}

	[TestMethod]
	public async Task Handle_ShouldUpdateProfilePicture_WhenImageIsUploadedSuccessfully()
	{
		// Arrange
		var user = new User { Id = 1, Name = "Test User", Email = "user@example.com", ProfilePictureUrl = "old-url.jpg" };
		var command = new UpdateUserProfilePictureCommand(1, new Mock<IFormFile>().Object);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserProfilePictureCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		// Mock the user repository to return the user
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Mock the image service to simulate a successful upload
		_imageServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new UploadResult { IsSuccess = true, Url = "new-url.jpg" });

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("new-url.jpg", user.ProfilePictureUrl);
		_userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task Handle_ShouldReturnError_WhenImageUploadFails()
	{
		// Arrange
		var user = new User { Id = 1, Name = "Test User", Email = "user@example.com", ProfilePictureUrl = "old-url.jpg" };
		var command = new UpdateUserProfilePictureCommand(1, new Mock<IFormFile>().Object);

		_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserProfilePictureCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		// Mock the user repository to return the user
		_userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Mock the image service to simulate a failed upload
		_imageServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new UploadResult { IsSuccess = false, Url = null });

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsTrue(result.IsError());
	}
}
