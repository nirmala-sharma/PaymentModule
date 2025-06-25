using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IPaymentTransactionService
    {
        public Task<Transaction> SaveTransaction(PaymentRequests request, DemoPaymentResponse response);
    }
}
