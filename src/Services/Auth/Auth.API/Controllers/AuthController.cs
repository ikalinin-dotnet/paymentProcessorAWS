using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Services.Auth.API.Models.DTOs;
using PaymentProcessor.Services.Auth.Domain.Repositories;
using PaymentProcessor.Services.Auth.Domain.Services;
using PaymentProcessor.BuildingBlocks.EventBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PaymentProcessor.Services.Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(
            IUserRepository userRepository,
            ITokenService tokenService,
            IEventBus eventBus,
            ILogger<AuthController> logger,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _eventBus = eventBus;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid registration data"
                });
            }

            try
            {
                var (user, token, refreshToken) = await _authService.RegisterAsync(
                    request.Email, 
                    request.Password, 
                    request.FirstName, 
                    request.LastName);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid login data"
                });
            }

            try
            {
                var (user, token, refreshToken) = await _authService.LoginAsync(
                    request.Email, 
                    request.Password);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid refresh token request"
                });
            }

            try
            {
                var (user, token, newRefreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Token refresh successful",
                    Token = token,
                    RefreshToken = newRefreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Token is required" });
            }

            var success = await _authService.RevokeTokenAsync(request.RefreshToken);
            if (!success)
            {
                return NotFound(new { message = "Token not found" });
            }

            return Ok(new { message = "Token revoked" });
        }
    }
}