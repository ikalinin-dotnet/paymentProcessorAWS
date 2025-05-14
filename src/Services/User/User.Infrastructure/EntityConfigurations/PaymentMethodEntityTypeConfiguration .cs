using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentProcessor.Services.User.Domain.ValueObjects;

namespace PaymentProcessor.Services.User.Infrastructure.EntityConfigurations;

public class PaymentMethodEntityTypeConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Last4)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(p => p.ExpiryDate);

        builder.Property(p => p.IsDefault)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}