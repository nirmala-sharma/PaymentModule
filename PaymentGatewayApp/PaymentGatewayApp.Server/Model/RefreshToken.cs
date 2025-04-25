using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayApp.Server.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }  
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
