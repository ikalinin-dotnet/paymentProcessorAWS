using System;

namespace PaymentProcessor.Services.Auth.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public RefreshToken()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

        public void Revoke()
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }
    }
}