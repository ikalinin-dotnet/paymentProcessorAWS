using Microsoft.Extensions.Logging;
using PaymentProcessor.BuildingBlocks.EventBus.Interfaces;
using PaymentProcessor.Services.User.Domain.Entities;
using PaymentProcessor.Services.User.Domain.Repositories;

namespace PaymentProcessor.Services.User.API.EventHandlers;

public class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IUserProfileRepository userProfileRepository,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling UserRegisteredEvent for user {UserId}", @event.UserId);
            
            // Check if user profile already exists
            var existingProfile = await _userProfileRepository.GetByUserIdAsync(@event.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning("User profile already exists for user {UserId}", @event.UserId);
                return;
            }

            // Create a new user profile
            var userProfile = new UserProfile(
                @event.UserId,
                @event.Email,
                @event.FirstName,
                @event.LastName,
                @event.PhoneNumber
            );

            await _userProfileRepository.CreateAsync(userProfile);
            _logger.LogInformation("Created user profile for user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserRegisteredEvent for user {UserId}", @event.UserId);
            throw;
        }
    }
}