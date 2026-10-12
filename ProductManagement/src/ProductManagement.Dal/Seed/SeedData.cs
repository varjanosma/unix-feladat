using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal.Entities;

namespace ProductManagement.Dal.Seed;

internal static class SeedData
{
    private static readonly DateTime ProductCreatedAtUtc = new(2025, 3, 14, 9, 0, 0, DateTimeKind.Utc);

    public static void ApplySeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductGroup>().HasData(ProductGroups);
        modelBuilder.Entity<Product>().HasData(Products);
        modelBuilder.Entity<ProductInfo>().HasData(ProductInfos);
        modelBuilder.Entity<User>().HasData(Users);
        modelBuilder.Entity<Purchase>().HasData(Purchases);
        modelBuilder.Entity<PurchaseItem>().HasData(PurchaseItems);
    }
    private static ProductGroup[] ProductGroups =>
        new[]
        {
            new ProductGroup { Id = 1, Code = "VEH", Name = "Vehicles", Description = "Used cars in stock" },
            new ProductGroup { Id = 2, Code = "TYR", Name = "Tyres and wheels", Description = "Summer, winter and all-season tyres, rims" },
            new ProductGroup { Id = 3, Code = "ENG", Name = "Engine and service", Description = "Filters, oils, service kits" },
            new ProductGroup { Id = 4, Code = "BRK", Name = "Brakes and suspension", Description = "Discs, pads, shock absorbers" },
            new ProductGroup { Id = 5, Code = "ACC", Name = "Accessories", Description = "Interior and exterior accessories" },
        };

    private static Product[] Products =>
        new[]
        {
            new Product { Id = 1, Sku = "VEH-001", Name = "Volkswagen Golf 1.6 TDI, 2019", ProductGroupId = 1, Price = 18900.00m, StockQuantity = 3, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 2, Sku = "VEH-002", Name = "Skoda Octavia Combi 2.0 TDI, 2021", ProductGroupId = 1, Price = 24500.00m, StockQuantity = 2, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 3, Sku = "TYR-001", Name = "Summer tyre 205/55 R16", ProductGroupId = 2, Price = 109.00m, StockQuantity = 64, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 4, Sku = "TYR-002", Name = "Winter tyre 195/65 R15", ProductGroupId = 2, Price = 89.50m, StockQuantity = 40, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 5, Sku = "TYR-003", Name = "Alloy wheel set, 16 inch", ProductGroupId = 2, Price = 420.00m, StockQuantity = 8, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 6, Sku = "ENG-001", Name = "Oil filter", ProductGroupId = 3, Price = 12.90m, StockQuantity = 150, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 7, Sku = "ENG-002", Name = "Timing belt kit", ProductGroupId = 3, Price = 89.00m, StockQuantity = 22, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },

            // Out of stock, so the product list has something to show for that state.
            new Product { Id = 8, Sku = "ENG-003", Name = "Engine oil 5W-30, 5 litres", ProductGroupId = 3, Price = 54.90m, StockQuantity = 0, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },

            new Product { Id = 9, Sku = "BRK-001", Name = "Brake disc set, front", ProductGroupId = 4, Price = 119.00m, StockQuantity = 18, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 10, Sku = "BRK-002", Name = "Brake pad set, rear", ProductGroupId = 4, Price = 46.50m, StockQuantity = 35, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },
            new Product { Id = 11, Sku = "ACC-001", Name = "All-weather floor mat set", ProductGroupId = 5, Price = 59.00m, StockQuantity = 45, IsActive = true, CreatedAtUtc = ProductCreatedAtUtc },

            // Inactive, still in stock - the two states are independent.
            new Product { Id = 12, Sku = "ACC-002", Name = "Roof box, 420 litres", ProductGroupId = 5, Price = 349.00m, StockQuantity = 6, IsActive = false, CreatedAtUtc = ProductCreatedAtUtc },
        };

    private static ProductInfo[] ProductInfos =>
        new[]
        {
            new ProductInfo { ProductId = 1, Manufacturer = "Volkswagen", Description = "Used hatchback, 1.6 TDI diesel, 115 hp, 142 000 km, full service history.", WarrantyMonths = 12, WeightKg = 1320.000m, CountryOfOrigin = "Germany" },
            new ProductInfo { ProductId = 2, Manufacturer = "Skoda", Description = "Used estate, 2.0 TDI diesel, 150 hp, 86 000 km, one previous owner.", WarrantyMonths = 12, WeightKg = 1465.000m, CountryOfOrigin = "Czech Republic" },
            new ProductInfo { ProductId = 3, Manufacturer = "Michelin", Description = "Summer tyre, 205/55 R16 91V, wet grip class A.", WarrantyMonths = 24, WeightKg = 9.200m, CountryOfOrigin = "France" },
            new ProductInfo { ProductId = 4, Manufacturer = "Continental", Description = "Winter tyre, 195/65 R15 91T, three-peak mountain snowflake marking.", WarrantyMonths = 24, WeightKg = 8.400m, CountryOfOrigin = "Germany" },
            new ProductInfo { ProductId = 5, Manufacturer = "Borbet", Description = "Set of four alloy wheels, 16 inch, 5x112 bolt pattern, silver.", WarrantyMonths = 36, WeightKg = 32.000m, CountryOfOrigin = "Germany" },
            new ProductInfo { ProductId = 6, Manufacturer = "Bosch", Description = "Spin-on oil filter, fits most 1.6 and 2.0 diesel engines.", WarrantyMonths = 12, WeightKg = 0.320m, CountryOfOrigin = "Hungary" },
            new ProductInfo { ProductId = 7, Manufacturer = "Gates", Description = "Timing belt kit with tensioner and idler pulley.", WarrantyMonths = 24, WeightKg = 1.850m, CountryOfOrigin = "Belgium" },
            new ProductInfo { ProductId = 8, Manufacturer = "Mobil", Description = "Fully synthetic engine oil, 5W-30, 5 litre canister.", WarrantyMonths = 0, WeightKg = 4.600m, CountryOfOrigin = "Netherlands" },
            new ProductInfo { ProductId = 9, Manufacturer = "Brembo", Description = "Pair of vented front brake discs, 288 mm.", WarrantyMonths = 24, WeightKg = 14.300m, CountryOfOrigin = "Italy" },
            new ProductInfo { ProductId = 10, Manufacturer = "Textar", Description = "Rear brake pad set, wear sensor included.", WarrantyMonths = 24, WeightKg = 2.100m, CountryOfOrigin = "Germany" },
            new ProductInfo { ProductId = 11, Manufacturer = "Petex", Description = "Rubber floor mat set for front and rear, universal fit.", WarrantyMonths = 12, WeightKg = 3.900m, CountryOfOrigin = "Poland" },
            new ProductInfo { ProductId = 12, Manufacturer = "Thule", Description = "Lockable roof box, 420 litres, dual side opening.", WarrantyMonths = 60, WeightKg = 16.000m, CountryOfOrigin = "Sweden" },
        };
        
    private static User[] Users =>
        new[]
        {
            new User { Id = 1, UserName = "jnovak", FullName = "Novák János", Email = "janos.novak@example.com", RegisteredAtUtc = new DateTime(2024, 5, 6, 8, 30, 0, DateTimeKind.Utc) },
            new User { Id = 2, UserName = "akiss", FullName = "Kiss Anna", Email = "anna.kiss@example.com", RegisteredAtUtc = new DateTime(2025, 1, 20, 14, 5, 0, DateTimeKind.Utc) },
            new User { Id = 3, UserName = "btoth", FullName = "Tóth Bence", Email = "bence.toth@example.com", RegisteredAtUtc = new DateTime(2025, 6, 2, 10, 45, 0, DateTimeKind.Utc) },
        };

    private static Purchase[] Purchases =>
        new[]
        {
            new Purchase { Id = 1, UserId = 1, PurchasedAtUtc = new DateTime(2025, 7, 2, 11, 12, 0, DateTimeKind.Utc), Note = "Summer tyre change" },

            // No note - the purchase list has to cope with that too.
            new Purchase { Id = 2, UserId = 1, PurchasedAtUtc = new DateTime(2025, 9, 18, 16, 40, 0, DateTimeKind.Utc) },

            new Purchase { Id = 3, UserId = 2, PurchasedAtUtc = new DateTime(2025, 8, 11, 9, 5, 0, DateTimeKind.Utc), Note = "Service parts" },
            new Purchase { Id = 4, UserId = 3, PurchasedAtUtc = new DateTime(2026, 2, 3, 13, 20, 0, DateTimeKind.Utc), Note = "Car purchase" },
            new Purchase { Id = 5, UserId = 2, PurchasedAtUtc = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc), Note = "Winter set" },
        };

    private static PurchaseItem[] PurchaseItems =>
        new[]
        {
            // 1 - summer tyre change
            new PurchaseItem { Id = 1, PurchaseId = 1, ProductId = 3, Quantity = 4, UnitPrice = 109.00m },
            new PurchaseItem { Id = 2, PurchaseId = 1, ProductId = 6, Quantity = 1, UnitPrice = 12.90m },

            // 2
            new PurchaseItem { Id = 3, PurchaseId = 2, ProductId = 7, Quantity = 1, UnitPrice = 89.00m },

            // 3 - service parts
            new PurchaseItem { Id = 4, PurchaseId = 3, ProductId = 9, Quantity = 1, UnitPrice = 119.00m },
            new PurchaseItem { Id = 5, PurchaseId = 3, ProductId = 10, Quantity = 1, UnitPrice = 46.50m },
            new PurchaseItem { Id = 6, PurchaseId = 3, ProductId = 8, Quantity = 1, UnitPrice = 54.90m },

            // 4 - car purchase
            new PurchaseItem { Id = 7, PurchaseId = 4, ProductId = 1, Quantity = 1, UnitPrice = 18900.00m },
            new PurchaseItem { Id = 8, PurchaseId = 4, ProductId = 11, Quantity = 1, UnitPrice = 59.00m },

            // 5 - winter set
            new PurchaseItem { Id = 9, PurchaseId = 5, ProductId = 4, Quantity = 4, UnitPrice = 89.50m },
            new PurchaseItem { Id = 10, PurchaseId = 5, ProductId = 5, Quantity = 1, UnitPrice = 420.00m },
        };
}
