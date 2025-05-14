using Microsoft.EntityFrameworkCore;
using PaymentProcessor.Services.Auth.Domain.Entities;
using PaymentProcessor.Services.Auth.Domain.Repositories;
using PaymentProcessor.Services.Auth.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _dbContext;
        
        public RefreshTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }
        
        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }
        
        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task<RefreshToken> GetActiveTokenForUserAsync(string userId)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && 
                                         !r.IsRevoked && 
                                          r.ExpiresAt > now);
        }
        
        public async Task RevokeUserTokensAsync(string userId)
        {
            var userTokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            
            foreach (var token in userTokens)
            {
                token.Revoke();
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
}