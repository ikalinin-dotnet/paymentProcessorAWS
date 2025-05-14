using PaymentProcessor.Services.User.Domain.Entities;

namespace PaymentProcessor.Services.User.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(string id);
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile?> GetByEmailAsync(string email);
    Task<bool> CreateAsync(UserProfile userProfile);
    Task<bool> UpdateAsync(UserProfile userProfile);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<UserProfile>> GetAllAsync();
}