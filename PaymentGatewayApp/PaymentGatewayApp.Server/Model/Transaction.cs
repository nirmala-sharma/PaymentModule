namespace PaymentGatewayApp.Server.Model
{
    public class Transaction
    {
        public Guid TransactionId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public User? User { get; set; }
        public Guid UserId { get; set; }
    }
}
