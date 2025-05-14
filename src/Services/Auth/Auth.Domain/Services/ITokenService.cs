using PaymentProcessor.Services.Auth.Domain.Entities;
using System.Collections.Generic;
using System.Security.Claims;

namespace PaymentProcessor.Services.Auth.Domain.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(ApplicationUser user, IEnumerable<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}