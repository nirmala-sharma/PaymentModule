namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IPasswordHasher
    {
        string GenerateHashPassword(string password);
        bool VerifyPassword(string requestPassword, string savedPassword);
    }
}
