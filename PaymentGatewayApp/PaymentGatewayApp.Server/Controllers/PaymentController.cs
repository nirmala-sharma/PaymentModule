using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;
using PaymentGatewayApp.Server.Services;

namespace PaymentGatewayApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentPublisher _eventPublisher;
        private readonly IPaymentTransactionService _transactionService;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIdempotencyKeyService _idempotencyService;
        private readonly ILogger<PaymentTransactionService> _logger;

        public PaymentController(IPaymentPublisher eventPublisher, IPaymentTransactionService transactionService, ApplicationDbContext context, IIdempotencyKeyService idempotencyService, ILogger<PaymentTransactionService> logger)
        {
            _eventPublisher = eventPublisher;
            _transactionService = transactionService;
            _applicationDbContext = context;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequests request)
        {
            using (var dbTransaction = await _applicationDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
        string.IsNullOrWhiteSpace(idempotencyKey))
                    {
                        return BadRequest(new { Message = "Missing or invalid Idempotency-Key header." });
                    }

                    // Step 1: Check if the key already exists
                    var existing = await _applicationDbContext.IdempotencyKeys.FindAsync(idempotencyKey);
                    if (existing != null)
                    {
                        // Return the stored response
                        return Content(existing.ResponseBody, "application/json");
                    }

                    var eventResponse = await _eventPublisher.PublishPaymentEvent(request);


                    if (string.IsNullOrEmpty(eventResponse))
                    {

                        return StatusCode(500, new { Message = "No response received from the payment processor." });
                    }

                    var paymentResponse = JsonConvert.DeserializeObject<DemoPaymentResponse>(eventResponse);

                    if (paymentResponse == null)
                    {

                        return StatusCode(500, new { Message = "Invalid response format from the payment processor." });
                    }
                    var transaction = await _transactionService.SaveTransaction(request, paymentResponse);

                    if (paymentResponse.Status == "Failed")
                    {
                        throw new Exception("Payment not success.");
                    }
                    var responseJson = JsonConvert.SerializeObject(paymentResponse);

                    var idempotentKey = new IdempotencyKey
                    {
                        Id = idempotencyKey.ToString(),
                        ResponseBody = responseJson,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _idempotencyService.SaveIdempotencyKey(idempotentKey);
                    await dbTransaction.CommitAsync();
                    return Ok(paymentResponse);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    await dbTransaction.RollbackAsync();
                    return StatusCode(500, new { Message = "An error occurred while processing the payment.", Error = ex.Message });
                }
            }
        }
    }
}