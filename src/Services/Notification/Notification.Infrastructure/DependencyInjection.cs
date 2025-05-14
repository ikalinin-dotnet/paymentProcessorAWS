using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Repositories;
using Notification.Domain.Services;
using Notification.Infrastructure.Data;
using Notification.Infrastructure.Events;
using Notification.Infrastructure.Repositories;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();

            // Register notification services
            services.AddScoped<IEmailSender, AwsEmailSender>();
            services.AddScoped<ISmsSender, AwsSmsSender>();

            // Register domain services
            services.AddScoped<INotificationService, NotificationService>();

            // Register Kafka event consumers
            services.AddHostedService<PaymentEventConsumer>();

            return services;
        }
    }
}