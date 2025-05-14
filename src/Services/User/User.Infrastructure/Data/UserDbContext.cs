using Microsoft.EntityFrameworkCore;
using PaymentProcessor.Services.User.Domain.Entities;
using PaymentProcessor.Services.User.Domain.ValueObjects;
using PaymentProcessor.Services.User.Infrastructure.EntityConfigurations;

namespace PaymentProcessor.Services.User.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserProfileEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AddressEntityTypeConfiguration());
    }
}