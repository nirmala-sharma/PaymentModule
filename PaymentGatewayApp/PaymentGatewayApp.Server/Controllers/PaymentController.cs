using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentPublisher _eventPublisher;
    
        public PaymentController( IPaymentPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequests request)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var eventResponse = await _eventPublisher.PublishPaymentEvent(request, correlationId);


                if (string.IsNullOrEmpty(eventResponse))
                {

                    return StatusCode(500, new { Message = "No response received from the payment processor." });
                }

                var paymentResponse = JsonConvert.DeserializeObject<DemoPaymentResponse>(eventResponse);

                if (paymentResponse == null)
                {

                    return StatusCode(500, new { Message = "Invalid response format from the payment processor." });
                }

                if (paymentResponse.Status == "Failed")
                {
                    throw new Exception("Payment not success.");
                }
                return Ok(paymentResponse);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the payment.", Error = ex.Message });
            }
        }
    }
}
