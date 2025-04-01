using Newtonsoft.Json;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Requests;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace PaymentGatewayApp.Server.Services
{
    /// <summary>
    /// The Payment Event Publisher is like a helpful postal worker for payment requests.
    /// Its job is to:
    /// 1. Accept payment requests from our system
    /// 2. Package them neatly for delivery to the payment processor
    /// 3. Wait patiently for the processor's reply
    /// 4. Deliver responses back to the right requestor
    /// </summary>
    public class PaymentEventPublisher : IPaymentPublisher
    {
        private readonly IModel _channel;
        private readonly string _requestQueueName = "thirdPartyApiRequestQueue";  // Outgoing mailbox
        private readonly string _responseQueueName = "thirdPartyApiResponseQueue"; // Incoming mailbox
        public PaymentEventPublisher(IModel channel)
        {
            _channel = channel;
            // Set up our message highways (Queues are like Mailbox for realworld analogy)
            _channel.QueueDeclare(queue: _requestQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null); // Request Queue (Outbound lane)
            _channel.QueueDeclare(queue: _responseQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null); // Response Queue (Return lane)
        }

        public async Task<string> PublishPaymentEvent(PaymentRequests request)
        {
            var correlationId = Guid.NewGuid().ToString();
            // Package our message with return address
            var properties = _channel.CreateBasicProperties();
            properties.ReplyTo = _responseQueueName;  // "Reply to this address"
            properties.CorrelationId = correlationId;  // "My ID for matching replies"
            var message = JsonConvert.SerializeObject(request);

            _channel.BasicPublish(exchange: "", routingKey: _requestQueueName, basicProperties: properties, body: Encoding.UTF8.GetBytes(message));

            // Set up a mailbox for our reply
            var tcs = new TaskCompletionSource<string>();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                // Only accept letters with our exact ID
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                    tcs.TrySetResult(response);
                }
            };
            // Start checking the mailbox
            var consumerTag = _channel.BasicConsume(queue: _responseQueueName, autoAck: true, consumer: consumer);  //  autoAck: true : Automatic thank-you notes
            // Wait patiently for our special letter
            var result = await tcs.Task;
            // Once the publisher consume the message then it Stop sending messages to this specific consumer , Basically it was done because
            // correlationId again updated to previous correlationId not the latest one so it is not giving the output
            _channel.BasicCancel(consumerTag);  
            return result;
        }
    }
}
