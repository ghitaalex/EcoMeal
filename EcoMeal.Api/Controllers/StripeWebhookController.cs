using EcoMeal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace EcoMeal.Api.Controllers
{
    [Route("api/payments/stripe")]
    [ApiController]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public StripeWebhookController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = _paymentService.GetWebhookEvent(
                    json,
                    Request.Headers["Stripe-Signature"].ToString());

                if (stripeEvent.Data.Object is not Session session)
                {
                    return Ok();
                }

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    await _paymentService.CompletePaymentAsync(session);
                }
                else if (stripeEvent.Type == "checkout.session.expired")
                {
                    await _paymentService.ExpirePaymentAsync(session);
                }

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
