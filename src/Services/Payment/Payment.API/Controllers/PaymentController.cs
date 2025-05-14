using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Models;
using Payment.Domain.Models;
using Payment.Domain.Services;
using System.Security.Claims;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                
                var transaction = await _paymentService.InitiatePaymentAsync(
                    userId,
                    request.Amount,
                    request.Currency,
                    request.PaymentMethodId);

                var response = new ProcessPaymentResponse
                {
                    TransactionId = transaction.Id,
                    Status = transaction.Status.ToString(),
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Timestamp = transaction.CreatedAt,
                    Success = transaction.Status == TransactionStatus.Successful,
                    ErrorMessage = transaction.ErrorMessage
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid payment request: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { error = "An error occurred while processing the payment." });
            }
        }

        [HttpGet("methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var paymentMethods = await _paymentService.GetUserPaymentMethodsAsync(userId);

                var response = paymentMethods.Select(pm => new PaymentMethodResponse
                {
                    Id = pm.Id,
                    Type = pm.Type.ToString(),
                    CardLastFour = pm.CardLastFour,
                    CardBrand = pm.CardBrand,
                    IsDefault = pm.IsDefault,
                    CreatedAt = pm.CreatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods");
                return StatusCode(500, new { error = "An error occurred while retrieving payment methods." });
            }
        }

        [HttpPost("methods")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                
                var paymentMethod = await _paymentService.AddPaymentMethodAsync(
                    userId,
                    request.Type,
                    request.Token,
                    request.SetAsDefault);

                var response = new PaymentMethodResponse
                {
                    Id = paymentMethod.Id,
                    Type = paymentMethod.Type.ToString(),
                    CardLastFour = paymentMethod.CardLastFour,
                    CardBrand = paymentMethod.CardBrand,
                    IsDefault = paymentMethod.IsDefault,
                    CreatedAt = paymentMethod.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while adding the payment method." });
            }
        }

        [HttpDelete("methods/{id}")]
        public async Task<IActionResult> DeletePaymentMethod(Guid id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _paymentService.DeletePaymentMethodAsync(id, userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid request to delete payment method: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment method");
                return StatusCode(500, new { error = "An error occurred while deleting the payment method." });
            }
        }

        [HttpPost("methods/{id}/default")]
        public async Task<IActionResult> SetDefaultPaymentMethod(Guid id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _paymentService.SetDefaultPaymentMethodAsync(id, userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid request to set default payment method: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default payment method");
                return StatusCode(500, new { error = "An error occurred while setting the default payment method." });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var transactions = await _paymentService.GetUserTransactionsAsync(userId);

                var response = transactions.Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    ExternalTransactionId = t.ExternalTransactionId,
                    PaymentMethod = new PaymentMethodResponse
                    {
                        Id = t.PaymentMethod.Id,
                        Type = t.PaymentMethod.Type.ToString(),
                        CardLastFour = t.PaymentMethod.CardLastFour,
                        CardBrand = t.PaymentMethod.CardBrand,
                        IsDefault = t.PaymentMethod.IsDefault,
                        CreatedAt = t.PaymentMethod.CreatedAt
                    }
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return StatusCode(500, new { error = "An error occurred while retrieving transactions." });
            }
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var transaction = await _paymentService.GetTransactionAsync(id);

                if (transaction == null || transaction.UserId != userId)
                {
                    return NotFound();
                }

                var response = new TransactionResponse
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt,
                    ExternalTransactionId = transaction.ExternalTransactionId,
                    PaymentMethod = new PaymentMethodResponse
                    {
                        Id = transaction.PaymentMethod.Id,
                        Type = transaction.PaymentMethod.Type.ToString(),
                        CardLastFour = transaction.PaymentMethod.CardLastFour,
                        CardBrand = transaction.PaymentMethod.CardBrand,
                        IsDefault = transaction.PaymentMethod.IsDefault,
                        CreatedAt = transaction.PaymentMethod.CreatedAt
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction");
                return StatusCode(500, new { error = "An error occurred while retrieving the transaction." });
            }
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("Invalid user ID in token");
            }
            
            return userId;
        }
    }
}