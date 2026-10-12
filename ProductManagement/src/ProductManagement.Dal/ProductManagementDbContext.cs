using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal.Entities;
using ProductManagement.Dal.Seed;

namespace ProductManagement.Dal;

/// <summary>
/// EF Core context of the system. Every table is exposed as a <see cref="DbSet{TEntity}"/>,
/// the mapping lives in the IEntityTypeConfiguration classes under Configurations, and
/// the sample data in <see cref="SeedData"/>.
/// </summary>
public class ProductManagementDbContext : DbContext
{
    public ProductManagementDbContext(DbContextOptions<ProductManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductInfo> ProductInfos => Set<ProductInfo>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapping: picks up every IEntityTypeConfiguration<T> in this assembly, so the
        // configuration of an entity lives in its own file.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductManagementDbContext).Assembly);

        // Content: the demo data, kept separate from the mapping.
        modelBuilder.ApplySeedData();
    }
}
