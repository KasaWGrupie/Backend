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
using KasaWGrupie.Persistence.Specifications.Groups;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Tests.Factories;
using KasaWGrupie.Infrastructure.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class AddUserToGroupHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepo;
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IValidator<AddUserToGroupCommand>> _validator;
        private Mock<IAuthService> _authService;
        private Mock<IHttpContextAccessor> _httpAccessor;
        private DefaultHttpContext _httpContext;
        private AddUserToGroupHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepo = new Mock<IRepositoryBase<Group>>();
            _userRepo = new Mock<IRepositoryBase<User>>();
            _validator = new Mock<IValidator<AddUserToGroupCommand>>();
            _authService = new Mock<IAuthService>();
            _httpAccessor = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _httpAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

            _handler = new AddUserToGroupHandler(
                _groupRepo.Object,
                _userRepo.Object,
                _validator.Object,
                _authService.Object,
                _httpAccessor.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new AddUserToGroupCommand(0, 5);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(cmd.GroupId), "must be > 0")
                }));

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenGroupNotFound()
        {
            var cmd = new AddUserToGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsUnauthorized_WhenRequesterNotAdmin()
        {
            var cmd = new AddUserToGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            // group with adminId = 10
            var admin = UserFactory.Create(id: 10);
            var group = new Group { Id = 1, Name="grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User>() };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync("foo@x.com");
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UserFactory.Create(id: 5, email: "foo@x.com"));

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Unauthorized);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenMemberUserNotFound()
        {
            var cmd = new AddUserToGroupCommand(1, 99);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 1, email: "a@x");
            var group = new Group { Id = 1, Name = "grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User>() };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(admin.Email);
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            _userRepo.Setup(r => r.GetByIdAsync(cmd.MemberId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_AddsMember_WhenAllValid()
        {
            var cmd = new AddUserToGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 1, email: "admin@x");
            var group = new Group { Id = 1, Name = "grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User>() };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(admin.Email);
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            var member = UserFactory.Create(id: 2, email:"member1@example.com");
            _userRepo.Setup(r => r.GetByIdAsync(cmd.MemberId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(member);


            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Ok);
            group.Members.Should().Contain(member);
            _groupRepo.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
            _groupRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [TestClass]
    public class RemoveUserFromGroupHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepo;
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IValidator<RemoveUserFromGroupCommand>> _validator;
        private Mock<IAuthService> _authService;
        private Mock<IHttpContextAccessor> _httpAccessor;
        private DefaultHttpContext _httpContext;
        private RemoveUserFromGroupHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepo = new Mock<IRepositoryBase<Group>>();
            _userRepo = new Mock<IRepositoryBase<User>>();
            _validator = new Mock<IValidator<RemoveUserFromGroupCommand>>();
            _authService = new Mock<IAuthService>();
            _httpAccessor = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _httpAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

            _handler = new RemoveUserFromGroupHandler(
                _groupRepo.Object,
                _userRepo.Object,
                _validator.Object,
                _authService.Object,
                _httpAccessor.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new RemoveUserFromGroupCommand(0, 5);
            _validator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(cmd.GroupId), "must be > 0")
                }));

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenGroupNotFound()
        {
            var cmd = new RemoveUserFromGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsUnauthorized_WhenRequesterNotAdmin()
        {
            var cmd = new RemoveUserFromGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 10);
            var group = new Group { Id = 1, Name = "grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User>() };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync("foo@x.com");
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UserFactory.Create(id: 5, email: "foo@x.com"));

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Unauthorized);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenMemberNotInGroup()
        {
            var cmd = new RemoveUserFromGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 1, email: "a@x");
            var group = new Group { Id = 1, Name = "grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User>() };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(admin.Email);
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_RemovesMember_WhenAllValid()
        {
            var cmd = new RemoveUserFromGroupCommand(1, 2);
            _validator.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 1, email: "admin@x");
            var member = UserFactory.Create(id: 2);
            var group = new Group { Id = 1, Name = "grupa", Currency = new Currency { Name = "EUR" }, Description = "", PictureUrl = "pic", Admin = admin, Members = new List<User> { member } };
            _groupRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Group>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            _authService.Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(admin.Email);
            _userRepo.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<UserByEmailSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Status.Should().Be(ResultStatus.Ok);
            group.Members.Should().NotContain(member);
            _groupRepo.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
            _groupRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
