using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Services;
using RabbitMQ.Client;

namespace PaymentGatewayApp.Server.Dependencies
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddService(this IServiceCollection services, ConfigurationManager configuration)
        {
            
            services.AddRabbmitMQConfiguration(configuration);

            return services;
        }
        public static IServiceCollection AddRabbmitMQConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<RabbitMQConfiguration>(configuration.GetSection(RabbitMQConfiguration.SectionName));

            services.AddSingleton<IConnection>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
                if (string.IsNullOrEmpty(options.HostName))
                {
                    throw new ArgumentNullException(nameof(options.HostName), "RabbitMQ HostName is not configured.");
                }

                // Create Connection with the RabbitMQ server
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                return factory.CreateConnection();
            });
            services.AddSingleton<IModel>(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel();
            });
            services.AddScoped<IPaymentPublisher, PaymentEventPublisher>();

            services.AddHttpClient();

            services.AddHostedService<PaymentEventConsumer>();  // Starts immediately with the app

            return services;
        }

    }
}
