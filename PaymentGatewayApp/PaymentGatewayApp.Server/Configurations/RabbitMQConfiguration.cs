namespace PaymentGatewayApp.Server.Configurations
{
    public class RabbitMQConfiguration
    {
        public const string SectionName = "RabbitMQ";

        public string HostName { get; set; } = "rabbitmq";//"localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public int Port { get; set; } = 5672; // Default RabbitMQ port
    }
}
