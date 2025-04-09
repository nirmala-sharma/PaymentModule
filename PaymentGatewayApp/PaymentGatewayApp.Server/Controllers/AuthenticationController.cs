using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJWTTokenGenerator _tokenGenerator;
        public AuthenticationController(IJWTTokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }
        [HttpPost("Login")]
        public IActionResult Login()
        {
            User user = new User();
            // user authentication logic here

            var token = _tokenGenerator.GenerateToken(user);
            var response = new AuthenticationResponse(user, token);
            return Ok(response);
        }
    }
}
