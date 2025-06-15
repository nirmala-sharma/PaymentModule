using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Services;
using System.Security.Claims;

namespace paymentAppTestProject
{
    public class AuthenticationServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            // Sets up an in-memory database instead of a real SQL database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);  // Uses the in-memory DB

            // Mocks HTTP context for simulating user claims
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Injects dependencies into service
            _authService = new AuthenticationService(_dbContext, _mockHttpContextAccessor.Object);
        }
        /// <summary>
        /// Verifies that GetUserByUserName returns the correct user 
        /// when a user with the specified username exists in the database.
        /// </summary>
        [Fact]
        public async Task GetUserByUserName_UserExists_ReturnsUser()
        {
            // Arrange
            var user = new User { UserId = Guid.NewGuid(), UserName = "niru" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _authService.GetUserByUserName("niru");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("niru", result.UserName);
        }

        /// <summary>
        /// Verifies that GetCurrentUserId correctly extracts and returns the user ID 
        /// from the NameIdentifier claim when a valid claim is present in the HTTP context.
        /// </summary>
        [Fact]
        public async Task GetCurrentUserId_ValidClaim_ReturnsUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext
            {
                User = principal
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _authService.GetCurrentUserId();

            // Assert
            Assert.Equal(userId, result);
        }
        /// <summary>
        /// Verifies that GetCurrentUserId returns null when the NameIdentifier claim 
        /// is missing from the HTTP context (i.e., the user identity has no claims).
        /// </summary>
        [Fact]
        public async Task GetCurrentUserId_InvalidClaim_ReturnsNull()
        {
            // Arrange: No claims in identity
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _authService.GetCurrentUserId();

            // Assert
            Assert.Null(result);
        }
    }
}
