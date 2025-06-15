using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentGatewayApp.Server;
using PaymentGatewayApp.Server.Controllers;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;

namespace PaymentAppTestProject
{
    public class AuthControllerTests
    {
        private readonly AuthenticationController _controller;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<IJWTTokenGenerator> _mockTokenGenerator;
        private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
        private readonly ApplicationDbContext _dbContext;

        public AuthControllerTests()
        {
            // Create in-memory database context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Setup mocks
            _mockAuthService = new Mock<IAuthenticationService>();  // Creates a fake version of the IAuthenticationService
            _mockTokenGenerator = new Mock<IJWTTokenGenerator>();
            _mockLogger = new Mock<ILogger<AuthenticationController>>();

            // Create controller
            _controller = new AuthenticationController(
                _mockAuthService.Object,
                _mockTokenGenerator.Object,
                _dbContext,
                _mockLogger.Object
            );

            // Setup fake HttpContext for cookies
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest("niru", "pass"); 

            _mockAuthService.Setup(s => s.GetUserByUserName("niru")).ReturnsAsync((User?)null); // tells it what to return when  GetUserByUserName() method is called

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, problemResult.StatusCode);
        }

        [Fact]
        public async Task Login_PasswordIncorrect_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest("niru", "wrongpassword");
            var user = new User { UserName = "niru", PasswordHash = "correctpass" };
            _mockAuthService.Setup(s => s.GetUserByUserName("niru")).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, problemResult.StatusCode);
        }

        [Fact]
        public async Task Login_ValidUser_ReturnsAccessToken()
        {
            // Arrange
            var loginRequest = new LoginRequest("niru", "niru123"); ;
            var user = new User { UserName = "niru", PasswordHash = "niru123" };

            _mockAuthService.Setup(s => s.GetUserByUserName("niru")).ReturnsAsync(user);
            _mockTokenGenerator.Setup(t => t.GenerateToken(user)).ReturnsAsync(("access123", "refresh456"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.NotNull(data);
            Assert.Equal("access123", data.accessToken);
        }
    }
}
