namespace Notification.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Recipient { get; private set; } // Email address, phone number, etc.
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public bool IsSent { get; private set; }
        public DateTime? SentAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Notification() { } // For EF Core

        public Notification(Guid userId, NotificationType type, string recipient, string subject, string body)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Type = type;
            Recipient = recipient;
            Subject = subject;
            Body = body;
            IsSent = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsSent()
        {
            IsSent = true;
            SentAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            IsSent = false;
            ErrorMessage = errorMessage;
        }
    }
}