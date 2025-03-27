namespace PaymentGatewayApp.Server.Configurations
{
    public class DemoPaymentAPISettings
    {
        public const string SectionName = "DemoPaymentAPISettings";
        public string APIUrl { get; init; } = "https://localhost:7046/api/DemoPayment/ProcessPayment";
    }
}
