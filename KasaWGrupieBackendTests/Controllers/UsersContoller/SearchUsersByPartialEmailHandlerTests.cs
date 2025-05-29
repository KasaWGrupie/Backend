using Ardalis.Result;
using Ardalis.Specification;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.Requests.Users.Commands;
using KasaWGrupie.API.Requests.Users.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Persistence.Specifications.Users;
using Moq;

namespace KasaWGrupieTests;

[TestClass]
public class SearchUsersByPartialEmailHandlerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IRepositoryBase<User>> _userRepositoryMock;
    private Mock<IValidator<SearchUsersByPartialEmailCommand>> _validatorMock;
    private SearchUsersByPartialEmailHandler _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IRepositoryBase<User>>();
        _validatorMock = new Mock<IValidator<SearchUsersByPartialEmailCommand>>();
        _handler = new SearchUsersByPartialEmailHandler(_userRepositoryMock.Object, _validatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidEmail_ReturnsSuccessWithUsers()
    {
        // Arrange
        var command = new SearchUsersByPartialEmailCommand("test");
        var users = new List<User>
        {
            new() { Id = 1, Name = "Test User 1", Email = "test1@example.com", ProfilePictureUrl = "url1.jpg" },
            new() { Id = 2, Name = "Test User 2", Email = "test2@example.com", ProfilePictureUrl = "url2.png" },
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(r => r.ListAsync(It.IsAny<SearchUsersByEmailPrefixSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be(1);
        result.Value[0].Name.Should().Be("Test User 1");
        result.Value[0].Email.Should().Be("test1@example.com");
        result.Value[0].ProfilePictureUrl.Should().Be("url1.jpg");
        result.Value[1].Id.Should().Be(2);
        result.Value[1].Name.Should().Be("Test User 2");
        result.Value[1].Email.Should().Be("test2@example.com");
        result.Value[1].ProfilePictureUrl.Should().Be("url2.png");
    }

    [TestMethod]
    public async Task Handle_WithInvalidEmail_ReturnsValidationErrors()
    {
        // Arrange
        var command = new SearchUsersByPartialEmailCommand("");
        var validationErrors = new List<ValidationFailure>
        {
            new("Email", "Email is invalid")
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().ContainSingle()
            .Which.Should().Match<ValidationError>(e => 
                e.Identifier == "Email" && 
                e.ErrorMessage == "Email is invalid");
    }

    [TestMethod]
    public async Task Handle_WithNoMatchingUsers_ReturnsEmptyList()
    {
        // Arrange
        var command = new SearchUsersByPartialEmailCommand("nonexistent");

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(r => r.ListAsync(It.IsAny<SearchUsersByEmailPrefixSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

}