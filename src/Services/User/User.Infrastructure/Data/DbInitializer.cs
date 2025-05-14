namespace PaymentProcessor.Services.User.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(UserDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Add any seed data if needed
    }
}