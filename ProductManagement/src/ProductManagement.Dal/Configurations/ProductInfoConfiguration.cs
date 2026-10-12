using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Dal.Entities;

namespace ProductManagement.Dal.Configurations;

internal sealed class ProductInfoConfiguration : IEntityTypeConfiguration<ProductInfo>
{
    public void Configure(EntityTypeBuilder<ProductInfo> builder)
    {
        // One-to-one: the primary key is the foreign key as well.
        builder.HasKey(x => x.ProductId);

        builder.HasOne(x => x.Product)
            .WithOne(x => x.Info)
            .HasForeignKey<ProductInfo>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Manufacturer).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.CountryOfOrigin).IsRequired().HasMaxLength(100);
        builder.Property(x => x.WeightKg).HasPrecision(10, 3);
    }
}
