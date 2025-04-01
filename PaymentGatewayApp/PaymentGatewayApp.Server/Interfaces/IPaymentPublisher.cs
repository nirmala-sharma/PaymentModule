using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IPaymentPublisher
    {
        Task<string> PublishPaymentEvent(PaymentRequests request);

    }
}
