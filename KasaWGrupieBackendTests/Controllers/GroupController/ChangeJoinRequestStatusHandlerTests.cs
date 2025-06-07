using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KasaWGrupie.API.Requests.Groups.Commands;
using KasaWGrupie.API.Requests.Groups.Handlers;
using KasaWGrupie.Core.Entities;
using KasaWGrupie.Core.Enums;
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Tests.Factories;
using KasaWGrupie.Infrastructure.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ardalis.Specification;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class ChangeJoinRequestStatusHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepo;
        private Mock<IRepositoryBase<JoinRequest>> _joinReqRepo;
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IValidator<ChangeJoinRequestStatusCommand>> _validator;
        private Mock<IAuthService> _authService;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private DefaultHttpContext _httpContext;
        private ChangeJoinRequestStatusHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepo = new Mock<IRepositoryBase<Group>>();
            _joinReqRepo = new Mock<IRepositoryBase<JoinRequest>>();
            _userRepo = new Mock<IRepositoryBase<User>>();
            _validator = new Mock<IValidator<ChangeJoinRequestStatusCommand>>();
            _authService = new Mock<IAuthService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _httpContext = new DefaultHttpContext();
            _httpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(_httpContext);

            _handler = new ChangeJoinRequestStatusHandler(
                _groupRepo.Object,
                _joinReqRepo.Object,
                _userRepo.Object,
                _validator.Object,
                _authService.Object,
                _httpContextAccessor.Object
            );
        }

        [TestMethod]
        public async Task Handle_ReturnsInvalid_WhenValidationFails()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 2, JoinRequestStatus.Confirmed);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult(new[]
              {
                  new ValidationFailure(nameof(cmd.GroupId), "fail")
              }));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Invalid);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenGroupNotFound()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 2, JoinRequestStatus.Confirmed);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());
            _groupRepo
              .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync((Group)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsUnauthorized_WhenUserIsNotAdmin()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 2, JoinRequestStatus.Rejected);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 10, email: "admin@x.com");
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "G",
                Description = "D",
                PictureUrl = "P",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync("notadmin@x.com");
            var notAdmin = UserFactory.Create(id: 20, email: "notadmin@x.com");
            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                  It.IsAny<UserByEmailSpecification>(),
                  It.IsAny<CancellationToken>()))
              .ReturnsAsync(notAdmin);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Unauthorized);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenJoinRequestNotFound()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 99, JoinRequestStatus.Confirmed);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 5, email: "admin@x.com");
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "G",
                Description = "D",
                PictureUrl = "P",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin.Email);
            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                  It.IsAny<UserByEmailSpecification>(),
                  It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);

            _joinReqRepo
              .Setup(r => r.GetByIdAsync(cmd.RequestId, It.IsAny<CancellationToken>()))
              .ReturnsAsync((JoinRequest)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_WhenJoinRequestGroupMismatch()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 2, JoinRequestStatus.Confirmed);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 7, email: "admin@x.com");
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "G",
                Description = "D",
                PictureUrl = "P",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin.Email);
            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                  It.IsAny<UserByEmailSpecification>(),
                  It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);

            var jr = new JoinRequest
            {
                Id = cmd.RequestId,
                RequestingUserId = 1,
                RequestingUser = UserFactory.Create(id: 1, email: "a@x.com"),
                GroupId = 999,  // mismatch
                Group = group,
                Status = JoinRequestStatus.Unconfirmed
            };
            _joinReqRepo
              .Setup(r => r.GetByIdAsync(cmd.RequestId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(jr);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccessAndUpdates_WhenAllIsWell()
        {
            var cmd = new ChangeJoinRequestStatusCommand(1, 2, JoinRequestStatus.Confirmed);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            var admin = UserFactory.Create(id: 1, email: "admin@x.com");
            var group = new Group
            {
                Id = cmd.GroupId,
                Name = "G",
                Description = "D",
                PictureUrl = "P",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.GetByIdAsync(cmd.GroupId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin.Email);
            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                  It.IsAny<UserByEmailSpecification>(),
                  It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);

            var jr = new JoinRequest
            {
                Id = cmd.RequestId,
                RequestingUserId = 1,
                RequestingUser = UserFactory.Create(id: 1, email: "a@x.com"),
                GroupId = cmd.GroupId,
                Group = group,
                Status = JoinRequestStatus.Unconfirmed
            };
            _joinReqRepo
              .Setup(r => r.GetByIdAsync(cmd.RequestId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(jr);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Ok);
            jr.Status.Should().Be(JoinRequestStatus.Confirmed);

            _joinReqRepo.Verify(r => r.UpdateAsync(jr, It.IsAny<CancellationToken>()), Times.Once);
            _joinReqRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
