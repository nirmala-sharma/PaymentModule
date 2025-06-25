using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Services
{
    public class IdempotencyKeyService : IIdempotencyKeyService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<IdempotencyKeyService> _logger;
        public IdempotencyKeyService(ApplicationDbContext dbContext, ILogger<IdempotencyKeyService> logger)
        {
            _applicationDbContext = dbContext;
            _logger = logger;
        }
        public async Task<IdempotencyKey> SaveIdempotencyKey(IdempotencyKey key)
        {
            using (var dbTransaction = await _applicationDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var idempotentKey = new IdempotencyKey
                    {
                        Id = key.Id.ToString(),
                        ResponseBody = key.ResponseBody,
                        CreatedAt = DateTime.UtcNow
                    };

                    _applicationDbContext.IdempotencyKeys.Add(idempotentKey);
                    await _applicationDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return idempotentKey;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
