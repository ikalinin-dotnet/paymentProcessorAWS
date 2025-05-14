namespace Notification.Domain.Models
{
    public class NotificationTemplate
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public NotificationType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private NotificationTemplate() { } // For EF Core

        public NotificationTemplate(string name, string subject, string body, NotificationType type)
        {
            Id = Guid.NewGuid();
            Name = name;
            Subject = subject;
            Body = body;
            Type = type;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string subject, string body)
        {
            Subject = subject;
            Body = body;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}