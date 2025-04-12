using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User>? GetUserByUserName(string userName);
        Task<Guid?> GetCurrentUserId();
    }
}
