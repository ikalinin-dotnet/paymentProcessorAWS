using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Events;
using Payment.Infrastructure.Repositories;
using Payment.Infrastructure.Services;

namespace Payment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext if using EF Core
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName)));

            // Choose between EF Core repositories or AWS DynamoDB repositories based on configuration
            if (configuration.GetValue<bool>("UseAWSDynamoDB"))
            {
                services.AddAWSService<IAmazonDynamoDB>();
                services.AddScoped<ITransactionRepository, DynamoDBTransactionRepository>();
                // Implement and register DynamoDB repository for payment methods as well
            }
            else
            {
                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            }

            // Register event publisher
            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

            // Register payment processor service
            services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();

            // Register domain services
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }
    }
}