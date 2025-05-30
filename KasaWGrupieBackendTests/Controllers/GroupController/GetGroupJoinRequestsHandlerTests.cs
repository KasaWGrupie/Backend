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
using KasaWGrupie.Persistence.Specifications.Users;
using KasaWGrupie.Persistence.Specifications.Groups;
using KasaWGrupie.Tests.Factories;
using KasaWGrupie.Infrastructure.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class GetGroupJoinRequestsHandlerTests
    {
        private Mock<IRepositoryBase<Group>> _groupRepo;
        private Mock<IRepositoryBase<JoinRequest>> _joinReqRepo;
        private Mock<IRepositoryBase<User>> _userRepo;
        private Mock<IValidator<GetGroupJoinRequestsCommand>> _validator;
        private Mock<IAuthService> _authService;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private DefaultHttpContext _httpContext;
        private GetGroupJoinRequestsHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _groupRepo = new Mock<IRepositoryBase<Group>>();
            _joinReqRepo = new Mock<IRepositoryBase<JoinRequest>>();
            _userRepo = new Mock<IRepositoryBase<User>>();
            _validator = new Mock<IValidator<GetGroupJoinRequestsCommand>>();
            _authService = new Mock<IAuthService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            // Provide a real HttpContext for the AuthService call
            _httpContext = new DefaultHttpContext();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

            _handler = new GetGroupJoinRequestsHandler(
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
            var cmd = new GetGroupJoinRequestsCommand(0);
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
        public async Task Handle_ReturnsNotFound_WhenGroupMissing()
        {
            var cmd = new GetGroupJoinRequestsCommand(7);
            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            _groupRepo
              .Setup(r => r.FirstOrDefaultAsync(
                 It.IsAny<ISpecification<Group>>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync((Group)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.NotFound);
        }

        [TestMethod]
        public async Task Handle_ReturnsUnauthorized_WhenUserIsNotAdmin()
        {
            var groupId = 7;
            var cmd = new GetGroupJoinRequestsCommand(groupId);

            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            // Create a group whose Admin.Id = 99
            var admin = UserFactory.Create(id: 99, email: "admin@x.com");
            var group = new Group
            {
                Id = groupId,
                Name = "G",
                Description = "D",
                PictureUrl = "pic.jpg",
                Currency = new Currency { Name = "EUR" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.FirstOrDefaultAsync(
                 It.IsAny<ISpecification<Group>>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            // AuthService returns a non-admin email
            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync("notadmin@x.com");

            // userRepo finds that non-admin user
            var notAdmin = UserFactory.Create(id: 42, email: "notadmin@x.com");
            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                 It.IsAny<UserByEmailSpecification>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(notAdmin);

            var uA = UserFactory.Create(id: 2, email: "a@x.com");
            var uB = UserFactory.Create(id: 3, email: "b@x.com");
            var jr1 = new JoinRequest
            {
                Id = 100,
                RequestingUserId = uA.Id,
                RequestingUser = uA,
                GroupId = groupId,
                Group = group,
                Status = JoinRequestStatus.Unconfirmed
            };
            var jr2 = new JoinRequest
            {
                Id = 101,
                RequestingUserId = uB.Id,
                RequestingUser = uB,
                GroupId = groupId,
                Group = group,
                Status = JoinRequestStatus.Confirmed
            };
            _joinReqRepo
              .Setup(r => r.ListAsync(
                 It.IsAny<JoinRequestsByGroupSpec>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<JoinRequest> { jr1, jr2 });

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Unauthorized);
        }

        [TestMethod]
        public async Task Handle_ReturnsSuccess_WithAllJoinRequests_WhenUserIsAdmin()
        {
            var groupId = 5;
            var cmd = new GetGroupJoinRequestsCommand(groupId);

            _validator
              .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ValidationResult());

            // Admin user
            var admin = UserFactory.Create(id: 1, email: "admin@x.com");
            var group = new Group
            {
                Id = groupId,
                Name = "TestGroup",
                Description = "Desc",
                PictureUrl = "pic.jpg",
                Currency = new Currency { Name = "PLN" },
                Admin = admin,
                Members = new List<User> { admin },
                Status = GroupStatus.Active
            };
            _groupRepo
              .Setup(r => r.FirstOrDefaultAsync(
                 It.IsAny<ISpecification<Group>>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(group);

            _authService
              .Setup(a => a.GetEmailFromAuthTokenAsync(_httpContext, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin.Email);

            _userRepo
              .Setup(r => r.FirstOrDefaultAsync(
                 It.IsAny<UserByEmailSpecification>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);

            // Two join-requests
            var uA = UserFactory.Create(id: 2, email: "a@x.com");
            var uB = UserFactory.Create(id: 3, email: "b@x.com");
            var jr1 = new JoinRequest
            {
                Id = 100,
                RequestingUserId = uA.Id,
                RequestingUser = uA,
                GroupId = groupId,
                Group = group,
                Status = JoinRequestStatus.Unconfirmed
            };
            var jr2 = new JoinRequest
            {
                Id = 101,
                RequestingUserId = uB.Id,
                RequestingUser = uB,
                GroupId = groupId,
                Group = group,
                Status = JoinRequestStatus.Confirmed
            };
            _joinReqRepo
              .Setup(r => r.ListAsync(
                 It.IsAny<JoinRequestsByGroupSpec>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<JoinRequest> { jr1, jr2 });

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Status.Should().Be(ResultStatus.Ok);

            var dto = result.Value!;
            dto.GroupId.Should().Be(groupId);
            dto.JoinRequests.Should().HaveCount(2);
            dto.JoinRequests.Should().ContainEquivalentOf(new JoinRequestDto(
                100, uA.Id, uA.Name, JoinRequestStatus.Unconfirmed));
            dto.JoinRequests.Should().ContainEquivalentOf(new JoinRequestDto(
                101, uB.Id, uB.Name, JoinRequestStatus.Confirmed));
        }
    }
}
