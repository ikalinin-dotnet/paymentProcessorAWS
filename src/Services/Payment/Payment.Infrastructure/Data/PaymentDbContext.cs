using Microsoft.EntityFrameworkCore;
using Payment.Domain.Models;

namespace Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(3);
                entity.Property(e => e.ExternalTransactionId).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardLastFour).HasMaxLength(4);
                entity.Property(e => e.CardBrand).HasMaxLength(20);
                entity.Property(e => e.ExternalPaymentMethodId).HasMaxLength(100);
            });
        }
    }
}