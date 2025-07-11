﻿
using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.Controllers;
using PaymentGatewayApp.Server.Requests;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DemoPaymentAPISettings _settings;
        private readonly string _requestQueueName = "thirdPartyApiRequestQueue";
        private readonly string _responseQueueName = "thirdPartyApiResponseQueue";
        private readonly ILogger<PaymentEventConsumer> _logger;


        public PaymentEventConsumer(IConnection connection, IHttpClientFactory httpClientFactory, IOptions<DemoPaymentAPISettings> settings, ILogger<PaymentEventConsumer> logger)
        {
            _connection = connection;    // Store the RabbitMQ connection
            _httpClientFactory = httpClientFactory;   
            _settings = settings.Value;  // Store API settings (e.g., API URL)
            _logger = logger;
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
            var paymentResponse = new DemoPaymentResponse();
            try
            {
                // Define a retry policy that handles HttpRequestException
                var retryPolicy = Policy
                        .Handle<HttpRequestException>()   // Retry only when an HTTP request fails due to a network error, timeout, or server unreachability
                        .WaitAndRetryAsync(
                         retryCount: 3,     // Try up to 3 times before giving up
                        // Wait time between each retry (exponential backoff: 2s, 4s, 8s)
                         sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        // Action to perform on each retry (e.g., logging)
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            // Log retry attempt number, delay, and exception message
                            _logger.LogInformation($"[Retry attempt {retryCount} after {timeSpan.TotalSeconds}s] Exception: {exception.Message}");
                        });

                 // Executes the HTTP request with retry support.
                 // If the async operation fails due to a transient error (e.g., network failure or server error),
                 // Polly will automatically retry the request based on the configured policy.
                 // Note: ExecuteAsync must be used for awaited asynchronous operations like HttpClient calls.
                 await retryPolicy.ExecuteAsync(async () =>
                {
                    var client = _httpClientFactory.CreateClient();
                    
                    // Step 2: Send the request
                    HttpResponseMessage response = await client.PostAsync(thirdPartyApiUrl, jsonContent);

                    response.EnsureSuccessStatusCode();
                    // Step 3: Handle errors
                    if (!response.IsSuccessStatusCode)
                    {
                        // Step 4: Read and parse the response
                        string errorString = await response.Content.ReadAsStringAsync();

                        throw new Exception(errorString);
                    }

                    var responseString = await response.Content.ReadAsStringAsync();

                    paymentResponse = JsonConvert.DeserializeObject<DemoPaymentResponse>(responseString);

                    // Step 5: Handle empty responses
                    return paymentResponse ?? new DemoPaymentResponse
                    {
                        Status = "Failed",
                        Message = "No valid response from API",
                        Timestamp = DateTime.UtcNow
                    };
                });
                return paymentResponse;
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
