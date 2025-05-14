using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Services;
using Payment.Domain.Repositories;
using System.IO;
using System.Threading.Tasks;
using Stripe;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<WebhookController> _logger;
        private readonly string _webhookSecret;

        //Stripe event type constants
        private const string EVENT_PAYMENT_INTENT_SUCCEEDED = "payment_intent.succeeded";
        private const string EVENT_PAYMENT_INTENT_PAYMENT_FAILED = "payment_intent.payment_failed";

        public WebhookController(
            IConfiguration configuration,
            IPaymentService paymentService,
            ITransactionRepository transactionRepository,
            ILogger<WebhookController> logger)
        {
            _configuration = configuration;
            _paymentService = paymentService;
            _transactionRepository = transactionRepository;
            _logger = logger;
            _webhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                // Handle the event based on its type
                switch (stripeEvent.Type)
                {
                    case EVENT_PAYMENT_INTENT_SUCCEEDED:
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;
                    case EVENT_PAYMENT_INTENT_PAYMENT_FAILED:
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;
                    // Add other event types as needed
                    default:
                        _logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Error processing Stripe webhook");
                return BadRequest();
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            if (paymentIntent == null)
            {
                _logger.LogWarning("Failed to cast event data to PaymentIntent");
                return;
            }

            _logger.LogInformation("Payment succeeded for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

            // You might want to update transaction status here if needed
            // This is useful for payments that were initiated client-side
            
            // Example:
            // Find transaction by external ID and update its status
            // var transactions = await _transactionRepository.FindByExternalIdAsync(paymentIntent.Id);
            // foreach (var transaction in transactions)
            // {
            //    transaction.MarkAsSuccessful(paymentIntent.Id);
            //    await _transactionRepository.UpdateAsync(transaction);
            // }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            if (paymentIntent == null)
            {
                _logger.LogWarning("Failed to cast event data to PaymentIntent");
                return;
            }

            _logger.LogInformation("Payment failed for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            
            // Similar to success handler, update any transactions as needed
            // var transactions = await _transactionRepository.FindByExternalIdAsync(paymentIntent.Id);
            // foreach (var transaction in transactions)
            // {
            //    transaction.MarkAsFailed("Payment failed");
            //    await _transactionRepository.UpdateAsync(transaction);
            // }
        }
    }
}