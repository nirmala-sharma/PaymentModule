
using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.Requests;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace PaymentGatewayApp.Server.Services
{
    /// <summary>
    /// Background service that implements a RabbitMQ message consumer for payment processing.
    /// Handles the complete workflow:
    /// 1.  Listens for payment requests from a RabbitMQ queue(thirdPartyApiRequestQueue)
    /// 2.  Processes each request by calling an external payment API
    /// 3.  Sends back the API’s response to another queue(thirdPartyApiResponseQueue)
    /// </summary>
    
    public class PaymentEventConsumer : BackgroundService
    {
        private IConnection _connection;
        private readonly HttpClient _httpClient;
        private readonly DemoPaymentAPISettings _settings;
        private readonly string _requestQueueName = "thirdPartyApiRequestQueue";
        private readonly string _responseQueueName = "thirdPartyApiResponseQueue";


        public PaymentEventConsumer(IConnection connection, HttpClient httpClient, IOptions<DemoPaymentAPISettings> settings, ILogger<PaymentEventConsumer> logger)
        {
            _connection = connection;    // Store the RabbitMQ connection
            _httpClient = httpClient;    // Store the HTTP client
            _settings = settings.Value;  // Store API settings (e.g., API URL)
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Step 1: Create a RabbitMQ channel (like a "communication line")
            using var _channel = _connection.CreateModel();

            // Step 2: Declare queues (create if they don’t exist)
            _channel.QueueDeclare(queue: _requestQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);  // Queue for incoming requests
            _channel.QueueDeclare(queue: _responseQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);  // Queue for outgoing responses

            // Step 3: Set up a "listener" for new messages
            var consumer = new EventingBasicConsumer(_channel);


            consumer.Received += async (_, args) =>
            {
                try
                {
                    // Step 4: Read the incoming message
                    var body = args.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var request = JsonConvert.DeserializeObject<PaymentRequests>(message);
                    if (request == null)
                    {
                        throw new Exception("Invalid PaymentRequest format");
                    }

                    // Step 5: Call the external payment API
                    var paymentResponse = await CallThirdPartyPaymentAPI(message);

                    // Step 6: Send the API’s response back to the sender
                    var responseProperties = _channel.CreateBasicProperties();
                    responseProperties.CorrelationId = args.BasicProperties.CorrelationId;  // Match request-reply

                    var responseBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(paymentResponse));

                    _channel.BasicPublish(exchange: "", routingKey: _responseQueueName, basicProperties: responseProperties, body: responseBody);

                    // Step 7: Confirm message processing
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // If something fails, reject the message (no retry)
                    _channel.BasicNack(args.DeliveryTag, false, false);
                }
            };

            // Step 8: Start listening to the request queue
            _channel.BasicConsume(queue: "thirdPartyApiRequestQueue", autoAck: false, consumer: consumer);

            // Step 9: Keep the service running until stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);  // Check every second
            }
        }

        private async Task<DemoPaymentResponse?> CallThirdPartyPaymentAPI(string paymentData)
        {
            // Step 1: Prepare the API request
            string thirdPartyApiUrl = _settings.APIUrl;  // Get URL from config

            var jsonContent = new StringContent(paymentData, Encoding.UTF8, "application/json");

            try
            {
                // Step 2: Send the request
                HttpResponseMessage response = await _httpClient.PostAsync(thirdPartyApiUrl, jsonContent);

                // Step 3: Handle errors
                if (!response.IsSuccessStatusCode)
                {
                    // Step 4: Read and parse the response
                    string errorString = await response.Content.ReadAsStringAsync();

                    throw new Exception(errorString);
                }

                var responseString = await response.Content.ReadAsStringAsync();

                var paymentResponse = JsonConvert.DeserializeObject<DemoPaymentResponse>(responseString);

                // Step 5: Handle empty responses
                // Step 5: Handle empty responses
                return paymentResponse ?? new DemoPaymentResponse
                {
                    Status = "Failed",
                    Message = "No valid response from API",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                // Step 6: Return a friendly error if something crashes
                return new DemoPaymentResponse
                {
                    Status = "Failed",
                    Message = $"Error occurred while processing payment: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

    }
}
