using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Interfaces
{
    public interface IIdempotencyKeyService
    {
        public Task<IdempotencyKey> SaveIdempotencyKey(IdempotencyKey key);
    }
}
