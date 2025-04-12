using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJWTTokenGenerator _tokenGenerator;
        public AuthenticationController(IAuthenticationService authenticationService, IJWTTokenGenerator tokenGenerator)
        {
            _authenticationService = authenticationService;
            _tokenGenerator = tokenGenerator;
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
            if(user.PasswordHash != loginRequest.Password)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "User not found.");
            }

            var token = _tokenGenerator.GenerateToken(user);
            var response = new AuthenticationResponse(user, token);
            return Ok(response);
        }
    }
}
