namespace PaymentGatewayApp.Server.Requests
{
    public class PaymentRequests
    {
        // Payment details
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;

        // Customer details
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Card details
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;

        // Bank details
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
    }
}
