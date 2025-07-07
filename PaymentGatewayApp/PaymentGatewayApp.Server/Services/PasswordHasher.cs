using Microsoft.Extensions.Options;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PaymentGatewayApp.Server.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int keySize = 64;
        private const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        private readonly PasswordHashSettings _passwordHashSettings;

        public PasswordHasher(IOptions<PasswordHashSettings> passwordHashSettings)
        {
            _passwordHashSettings = passwordHashSettings.Value;
        }

        public string GenerateHashPassword(string password)
        {
            return HashPassword(password);
        }

        public bool VerifyPassword(string requestPassword, string savedPassword)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(_passwordHashSettings.Salt);
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(requestPassword, saltBytes, iterations, hashAlgorithm, keySize);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(savedPassword));
        }

        public string HashPassword(string password)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(_passwordHashSettings.Salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, iterations, hashAlgorithm, keySize);
            return Convert.ToHexString(hash);
        }

    }
}
