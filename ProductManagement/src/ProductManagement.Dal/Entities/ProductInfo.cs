namespace ProductManagement.Dal.Entities;

/// <summary>
/// Descriptive details of a product, kept in a separate table. One-to-one with
/// <see cref="Product"/>: the primary key is also the foreign key.
/// </summary>
public class ProductInfo
{
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int WarrantyMonths { get; set; }

    public decimal WeightKg { get; set; }

    public string CountryOfOrigin { get; set; } = string.Empty;
}
