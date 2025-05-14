using Microsoft.AspNetCore.Identity;
using PaymentProcessor.BuildingBlocks.EventBus.Interfaces;
using PaymentProcessor.Services.Auth.API.Events;
using PaymentProcessor.Services.Auth.Domain.Entities;
using PaymentProcessor.Services.Auth.Domain.Repositories;
using PaymentProcessor.Services.Auth.Domain.Services;
using System;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IEventBus _eventBus;
        
        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IEventBus eventBus)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _eventBus = eventBus;
        }
        
        public async Task<(ApplicationUser User, string Token, string RefreshToken)> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }
            
            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Invalid email or password");
            }
            
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            
            var roles = await _userRepository.GetUserRolesAsync(user);
            var jwtToken = _tokenService.GenerateJwtToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Create refresh token entity
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            
            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);
            
            // Publish user logged in event
            await _eventBus.PublishAsync(new UserLoggedInEvent
            {
                UserId = user.Id,
                Email = user.Email,
                LoginTime = DateTime.UtcNow
            });
            
            return (user, jwtToken, refreshToken);
        }
        
        public async Task<(ApplicationUser User, string Token, string RefreshToken)> RegisterAsync(
            string email, string password, string firstName, string lastName)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered");
            }
            
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true, // For simplicity, you might want to implement email verification
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await _userRepository.CreateAsync(newUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User registration failed: {errors}");
            }
            
            // Assign default role
            await _userRepository.AddToRoleAsync(newUser, "User");
            
            var roles = await _userRepository.GetUserRolesAsync(newUser);
            var jwtToken = _tokenService.GenerateJwtToken(newUser, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Create refresh token entity
            var refreshTokenEntity = new RefreshToken
            {
                UserId = newUser.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            
            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);
            
            // Publish user registered event
            await _eventBus.PublishAsync(new UserRegisteredEvent
            {
                UserId = newUser.Id,
                Email = newUser.Email
            });
            
            return (newUser, jwtToken, refreshToken);
        }
        
        public async Task<(ApplicationUser User, string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null || token.IsRevoked || DateTime.UtcNow >= token.ExpiresAt)
            {
                throw new InvalidOperationException("Invalid or expired refresh token");
            }
            
            var user = await _userRepository.GetByIdAsync(token.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            
            // Revoke current refresh token
            token.Revoke();
            await _refreshTokenRepository.UpdateAsync(token);
            
            // Generate new tokens
            var roles = await _userRepository.GetUserRolesAsync(user);
            var jwtToken = _tokenService.GenerateJwtToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            
            // Create new refresh token entity
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            
            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);
            
            return (user, jwtToken, newRefreshToken);
        }
        
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
            {
                return false;
            }
            
            token.Revoke();
            await _refreshTokenRepository.UpdateAsync(token);
            return true;
        }
    }
}