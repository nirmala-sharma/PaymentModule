using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayApp.Server.Model
{
    public class IdempotencyKey
    {
        [Key]
        public string Id { get; set; }

        public string ResponseBody { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
