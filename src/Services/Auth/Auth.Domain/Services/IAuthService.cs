using PaymentProcessor.Services.Auth.Domain.Entities;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.Domain.Services
{
    public interface IAuthService
    {
        Task<(ApplicationUser User, string Token, string RefreshToken)> LoginAsync(string email, string password);
        Task<(ApplicationUser User, string Token, string RefreshToken)> RegisterAsync(string email, string password, string firstName, string lastName);
        Task<(ApplicationUser User, string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}