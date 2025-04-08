using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Requests
{
    public record AuthenticationResponse(User user,string token);
}

