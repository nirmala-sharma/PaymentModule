using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IJWTTokenGenerator
    {
        Task<(string accessToken, string refreshToken)> GenerateToken(User user);
    }
}
