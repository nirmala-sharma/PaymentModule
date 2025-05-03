using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;
using System.IdentityModel.Tokens.Jwt;

namespace PaymentGatewayApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJWTTokenGenerator _tokenGenerator;
        private readonly ApplicationDbContext _context;
        public AuthenticationController(IAuthenticationService authenticationService, IJWTTokenGenerator tokenGenerator, ApplicationDbContext context)
        {
            _authenticationService = authenticationService;
            _tokenGenerator = tokenGenerator;
            _context = context;

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            await Task.CompletedTask;
            var user = await _authenticationService.GetUserByUserName(loginRequest.UserName);
            if (user is null)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "User not found.");
            }
            if (user.PasswordHash != loginRequest.Password)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "User not found.");
            }

            (string accessToken, string refreshToken) = await _tokenGenerator.GenerateToken(user);
            var response = new AuthenticationResponse(user.UserId,accessToken,refreshToken);
            return Ok(response);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRequest request)
        {
            var storedToken = await _context.RefreshToken
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid Refresh Token");

            // Validate the old JWT (optional - verify it matches)
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(request.AccessToken);
            if (storedToken.JwtId != jwtToken.Id)
                return Unauthorized("Token doesn't match");

            storedToken.IsUsed = true;
            _context.RefreshToken.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == storedToken.UserId);
             (string newAccessToken, string newRefreshToken) = await _tokenGenerator.GenerateToken(user);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
    }
}
