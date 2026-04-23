namespace OrderFlow.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();

        builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<OrderStatus>(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
