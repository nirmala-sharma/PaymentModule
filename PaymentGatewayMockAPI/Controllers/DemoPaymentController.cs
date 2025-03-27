using Microsoft.AspNetCore.Mvc;

namespace DemoPaymentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoPaymentController : ControllerBase
    {
        private static readonly string[] Statuses = { "Success", "Pending", "Failed" };

        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment()
        {
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

