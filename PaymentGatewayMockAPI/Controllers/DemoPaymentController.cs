using Microsoft.AspNetCore.Mvc;

namespace DemoPaymentAPI.Controllers
{
    /// <summary>
    /// Simulates payment processing with a mock third-party payment API.
    /// - Mimics real-world API behavior by introducing a 3-second delay.
    /// - Randomly returns transaction statuses such as Success, Pending, or Failed.
    /// - Useful for testing retry mechanisms, idempotency handling, and payment workflows
    ///   without relying on a real external service.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DemoPaymentController : ControllerBase
    {
        private static readonly string[] Statuses = { "Success", "Pending", "Failed" };

        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment()
        {
            //throw new HttpRequestException("Simulated failure for retry test");
            await Task.Delay(3000);
            Random random = new Random();

            string status = Statuses[random.Next(Statuses.Length)];

            return Ok(new
            {
                Status = status,
                Message = "Mock payment processing complete.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

