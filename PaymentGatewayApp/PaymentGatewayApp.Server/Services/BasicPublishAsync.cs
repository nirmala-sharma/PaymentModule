using Microsoft.EntityFrameworkCore.Metadata;

namespace PaymentGatewayApp.Server.Services
{
    internal class BasicPublishAsync
    {
        private IModel channel;

        public BasicPublishAsync(IModel channel)
        {
            this.channel = channel;
        }
    }
}