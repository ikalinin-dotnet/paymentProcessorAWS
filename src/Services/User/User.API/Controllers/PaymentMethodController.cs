using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentProcessor.Services.User.API.Models.DTOs;
using PaymentProcessor.Services.User.Domain.Repositories;
using PaymentProcessor.Services.User.Domain.ValueObjects;
using System.Security.Claims;

namespace PaymentProcessor.Services.User.API.Controllers;

[ApiController]
[Route("api/user-profiles/{profileId}/payment-methods")]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<PaymentMethodController> _logger;

    public PaymentMethodController(
        IUserProfileRepository userProfileRepository,
        ILogger<PaymentMethodController> logger)
    {
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodResponse>>> GetAll(string profileId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userProfile = await _userProfileRepository.GetByIdAsync(profileId);
        if (userProfile == null)
            return NotFound("User profile not found");

        // Ensure user can only access their own payment methods
        if (userProfile.UserId != userId)
            return Forbid();

        var paymentMethods = userProfile.PaymentMethods.Select(pm => new PaymentMethodResponse
        {
            Id = pm.Id,
            Type = pm.Type,
            Last4 = pm.Last4,
            Name = pm.Name,
            IsDefault = pm.IsDefault,
            ExpiryDate = pm.ExpiryDate
        });

        return Ok(paymentMethods);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodResponse>> Create(string profileId, CreatePaymentMethodRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userProfile = await _userProfileRepository.GetByIdAsync(profileId);
        if (userProfile == null)
            return NotFound("User profile not found");

        // Ensure user can only add payment methods to their own profile
        if (userProfile.UserId != userId)
            return Forbid();

        try
        {
            // In a real application, this would involve tokenizing the card with a payment provider
            // For this example, we're just storing a mock token
            var paymentMethod = new PaymentMethod(
                request.Type,
                $"token_{Guid.NewGuid():N}",  // Mock token
                request.Name,
                request.Last4,
                request.ExpiryDate,
                userProfile.PaymentMethods.Count == 0  // Make default if it's the first payment method
            );

            userProfile.AddPaymentMethod(paymentMethod);

            var updated = await _userProfileRepository.UpdateAsync(userProfile);
            if (!updated)
                return StatusCode(500, "Failed to add payment method");

            _logger.LogInformation("Added payment method to profile {ProfileId} for user {UserId}", profileId, userId);

            return Created($"/api/user-profiles/{profileId}/payment-methods/{paymentMethod.Id}", new PaymentMethodResponse
            {
                Id = paymentMethod.Id,
                Type = paymentMethod.Type,
                Last4 = paymentMethod.Last4,
                Name = paymentMethod.Name,
                IsDefault = paymentMethod.IsDefault,
                ExpiryDate = paymentMethod.ExpiryDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method to profile {ProfileId} for user {UserId}", profileId, userId);
            return StatusCode(500, "An error occurred while adding the payment method");
        }
    }

    [HttpDelete("{paymentMethodId}")]
    public async Task<ActionResult> Delete(string profileId, string paymentMethodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userProfile = await _userProfileRepository.GetByIdAsync(profileId);
        if (userProfile == null)
            return NotFound("User profile not found");

        // Ensure user can only delete payment methods from their own profile
        if (userProfile.UserId != userId)
            return Forbid();

        try
        {
            userProfile.RemovePaymentMethod(paymentMethodId);

            var updated = await _userProfileRepository.UpdateAsync(userProfile);
            if (!updated)
                return StatusCode(500, "Failed to remove payment method");

            _logger.LogInformation("Removed payment method {PaymentMethodId} from profile {ProfileId} for user {UserId}", paymentMethodId, profileId, userId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment method {PaymentMethodId} not found in profile {ProfileId}", paymentMethodId, profileId);
            return NotFound("Payment method not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment method {PaymentMethodId} from profile {ProfileId} for user {UserId}", paymentMethodId, profileId, userId);
            return StatusCode(500, "An error occurred while removing the payment method");
        }
    }

    [HttpPut("{paymentMethodId}/set-default")]
    public async Task<ActionResult> SetDefault(string profileId, string paymentMethodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userProfile = await _userProfileRepository.GetByIdAsync(profileId);
        if (userProfile == null)
            return NotFound("User profile not found");

        // Ensure user can only modify payment methods in their own profile
        if (userProfile.UserId != userId)
            return Forbid();

        try
        {
            userProfile.SetDefaultPaymentMethod(paymentMethodId);

            var updated = await _userProfileRepository.UpdateAsync(userProfile);
            if (!updated)
                return StatusCode(500, "Failed to set payment method as default");

            _logger.LogInformation("Set payment method {PaymentMethodId} as default for profile {ProfileId}", paymentMethodId, profileId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment method {PaymentMethodId} not found in profile {ProfileId}", paymentMethodId, profileId);
            return NotFound("Payment method not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting payment method {PaymentMethodId} as default for profile {ProfileId}", paymentMethodId, profileId);
            return StatusCode(500, "An error occurred while setting the payment method as default");
        }
    }
}