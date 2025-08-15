namespace PaymentGatewayApp.Server.Configurations
{
    public class RabbitMQConfiguration
    {
        public const string SectionName = "RabbitMQ";
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
