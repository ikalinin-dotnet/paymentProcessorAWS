using Notification.Domain.Models;
using Notification.Domain.Repositories;
using System.Text.RegularExpressions;

namespace Notification.Domain.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            INotificationTemplateRepository templateRepository,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _notificationRepository = notificationRepository;
            _templateRepository = templateRepository;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        public async Task<Models.Notification> SendEmailAsync(Guid userId, string email, string subject, string body)
        {
            var notification = new Models.Notification(userId, NotificationType.Email, email, subject, body);
            await _notificationRepository.CreateAsync(notification);

            try
            {
                var success = await _emailSender.SendEmailAsync(email, subject, body);
                
                if (success)
                {
                    notification.MarkAsSent();
                }
                else
                {
                    notification.MarkAsFailed("Email sending failed");
                }
                
                await _notificationRepository.UpdateAsync(notification);
                return notification;
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed(ex.Message);
                await _notificationRepository.UpdateAsync(notification);
                return notification;
            }
        }

        public async Task<Models.Notification> SendSmsAsync(Guid userId, string phoneNumber, string message)
        {
            var notification = new Models.Notification(userId, NotificationType.SMS, phoneNumber, string.Empty, message);
            await _notificationRepository.CreateAsync(notification);

            try
            {
                var success = await _smsSender.SendSmsAsync(phoneNumber, message);
                
                if (success)
                {
                    notification.MarkAsSent();
                }
                else
                {
                    notification.MarkAsFailed("SMS sending failed");
                }
                
                await _notificationRepository.UpdateAsync(notification);
                return notification;
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed(ex.Message);
                await _notificationRepository.UpdateAsync(notification);
                return notification;
            }
        }

        public async Task<Models.Notification> SendByTemplateAsync(Guid userId, string templateName, string recipient, Dictionary<string, string> parameters)
        {
            var template = await _templateRepository.GetByNameAsync(templateName);
            
            if (template == null)
            {
                throw new InvalidOperationException($"Template '{templateName}' not found");
            }

            var subject = ReplaceParameters(template.Subject, parameters);
            var body = ReplaceParameters(template.Body, parameters);

            switch (template.Type)
            {
                case NotificationType.Email:
                    return await SendEmailAsync(userId, recipient, subject, body);
                case NotificationType.SMS:
                    return await SendSmsAsync(userId, recipient, body);
                default:
                    throw new NotImplementedException($"Notification type {template.Type} not implemented");
            }
        }

        public async Task<IEnumerable<Models.Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _notificationRepository.GetByUserIdAsync(userId);
        }

        public async Task<NotificationTemplate> GetTemplateByNameAsync(string name)
        {
            return await _templateRepository.GetByNameAsync(name);
        }

        public async Task<NotificationTemplate> CreateTemplateAsync(string name, string subject, string body, NotificationType type)
        {
            var template = new NotificationTemplate(name, subject, body, type);
            await _templateRepository.CreateAsync(template);
            return template;
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(Guid id, string subject, string body)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            
            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID '{id}' not found");
            }

            template.Update(subject, body);
            await _templateRepository.UpdateAsync(template);
            return template;
        }

        private string ReplaceParameters(string content, Dictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return content;
            }

            foreach (var param in parameters)
            {
                content = content.Replace($"{{{param.Key}}}", param.Value);
            }

            return content;
        }
    }
}