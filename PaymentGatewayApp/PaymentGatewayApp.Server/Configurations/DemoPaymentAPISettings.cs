namespace PaymentGatewayApp.Server.Configurations
{
    public class DemoPaymentAPISettings
    {
        public const string SectionName = "DemoPaymentAPISettings";
        //public string APIUrl { get; init; } = "https://localhost:7046/api/DemoPayment/ProcessPayment";
        public string APIUrl { get; init; } = "http://mockapi:8080/api/DemoPayment/ProcessPayment";

    }
}
