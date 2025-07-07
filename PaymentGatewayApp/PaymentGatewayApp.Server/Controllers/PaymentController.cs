using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;
using PaymentGatewayApp.Server.Requests;
using PaymentGatewayApp.Server.Services;
using PaymentGatewayApp.Server.Validators;

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

        [EnableRateLimiting("FixedPolicy")]
        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequests request)
        {
            var validator = new PaymentRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                _logger.LogError(string.Join(",", errors));
                return BadRequest(new { Errors = errors });
            }
            try
            {
                // Validate the presence and integrity of the Idempotency-Key in the request header
                if (!HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
                 string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    return BadRequest(new { Message = "Missing or invalid Idempotency-Key header." });
                }

                // Check if a request with the same Idempotency-Key has already been processed successfully
                var existing = await _applicationDbContext.IdempotencyKeys.FindAsync(idempotencyKey);
                if (existing != null)
                {
                    // If found, return the previously stored response to ensure idempotency
                    return Content(existing.ResponseBody, "application/json");
                }

                // Publish the payment event to the message broker (RabbitMQ)
                var eventResponse = await _eventPublisher.PublishPaymentEvent(request);


                if (string.IsNullOrEmpty(eventResponse))
                {
                    // No response from the payment processor — internal failure
                    return StatusCode(500, new { Message = "No response received from the payment processor." });
                }

                var paymentResponse = JsonConvert.DeserializeObject<DemoPaymentResponse>(eventResponse);

                if (paymentResponse == null)
                {
                    // If deserialization fails, the format was invalid or corrupted
                    return StatusCode(500, new { Message = "Invalid response format from the payment processor." });
                }
                var transaction = await _transactionService.SaveTransaction(request, paymentResponse);

                if (paymentResponse.Status == "Failed")
                {
                    throw new Exception("Payment not success.");
                }
                var responseJson = JsonConvert.SerializeObject(paymentResponse);

                // Save the idempotency key and associated response for future safe replays
                var idempotentKey = new IdempotencyKey
                {
                    Id = idempotencyKey.ToString(),
                    ResponseBody = responseJson,
                    CreatedAt = DateTime.UtcNow
                };
                await _idempotencyService.SaveIdempotencyKey(idempotentKey);
                return Ok(paymentResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, new { Message = "An error occurred while processing the payment.", Error = ex.Message });
            }
        }
    }
}