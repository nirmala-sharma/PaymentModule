using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PaymentGatewayApp.Server.Services
{
    public class JWTTokenGenerator : IJWTTokenGenerator
    {
        private readonly JWTSettings _jwtSettings;
        private readonly ApplicationDbContext _context;
        public JWTTokenGenerator(IOptions<JWTSettings> jwtOptions, ApplicationDbContext context)
        {
            _jwtSettings = jwtOptions.Value;
            _context = context;

        }
        public async Task<(string accessToken, string refreshToken)> GenerateToken(User user)
        {
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)), SecurityAlgorithms.HmacSha256);
            var claims = new Claim[]
            {
              new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
              new Claim(ClaimTypes.UserData, user.UserName.ToString()),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = signingCredentials
            };
            var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // 2. Create Refresh Token
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                JwtId = token.Id,
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            await _context.RefreshToken.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken.Token);
        }
    }
}
