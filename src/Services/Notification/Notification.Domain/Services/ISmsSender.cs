namespace Notification.Domain.Services
{
    public interface ISmsSender
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}