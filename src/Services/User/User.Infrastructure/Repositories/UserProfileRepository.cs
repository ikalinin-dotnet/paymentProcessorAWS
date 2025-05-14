using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Services.User.Domain.Entities;
using PaymentProcessor.Services.User.Domain.Repositories;
using PaymentProcessor.Services.User.Infrastructure.Data;

namespace PaymentProcessor.Services.User.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<UserProfileRepository> _logger;

    public UserProfileRepository(
        UserDbContext dbContext,
        ILogger<UserProfileRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserProfile?> GetByIdAsync(string id)
    {
        try
        {
            return await _dbContext.UserProfiles
                .Include(u => u.Address)
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        try
        {
            return await _dbContext.UserProfiles
                .Include(u => u.Address)
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile with user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfile?> GetByEmailAsync(string email)
    {
        try
        {
            return await _dbContext.UserProfiles
                .Include(u => u.Address)
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile with email {Email}", email);
            throw;
        }
    }

    public async Task<bool> CreateAsync(UserProfile userProfile)
    {
        try
        {
            _dbContext.UserProfiles.Add(userProfile);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user profile for user ID {UserId}", userProfile.UserId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UserProfile userProfile)
    {
        try
        {
            _dbContext.UserProfiles.Update(userProfile);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile with ID {Id}", userProfile.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var userProfile = await _dbContext.UserProfiles.FindAsync(id);
            if (userProfile == null)
                return false;

            _dbContext.UserProfiles.Remove(userProfile);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        try
        {
            return await _dbContext.UserProfiles
                .Include(u => u.Address)
                .Include(u => u.PaymentMethods)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all user profiles");
            throw;
        }
    }
}