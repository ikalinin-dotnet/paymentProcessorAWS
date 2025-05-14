using PaymentProcessor.Services.Auth.Domain.Entities;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetActiveTokenForUserAsync(string userId);
        Task RevokeUserTokensAsync(string userId);
    }
}