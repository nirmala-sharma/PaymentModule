using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IJWTTokenGenerator
    {
        string GenerateToken(User user);
    }
}
