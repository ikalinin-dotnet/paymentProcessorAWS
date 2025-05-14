using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentProcessor.Services.User.Domain.Repositories;
using PaymentProcessor.Services.User.Infrastructure.Data;
using PaymentProcessor.Services.User.Infrastructure.Repositories;

namespace PaymentProcessor.Services.User.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        return services;
    }
}