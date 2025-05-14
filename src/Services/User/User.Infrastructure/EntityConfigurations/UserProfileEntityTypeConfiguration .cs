using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentProcessor.Services.User.Domain.Entities;

namespace PaymentProcessor.Services.User.Infrastructure.EntityConfigurations;

public class UserProfileEntityTypeConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .IsRequired();

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.HasIndex(u => u.UserId)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.OwnsOne(u => u.Address, a =>
        {
            a.WithOwner();
        });

        builder.OwnsMany(u => u.PaymentMethods, pm =>
        {
            pm.WithOwner().HasForeignKey("UserProfileId");
            pm.Property<string>("UserProfileId").IsRequired();
            pm.HasKey("Id", "UserProfileId");
        });
    }
}