namespace OrderFlow.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Sku).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Sku).IsUnique();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.StockQuantity).IsRequired();

        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
