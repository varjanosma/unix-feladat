namespace ProductManagement.Dal.Entities;

/// <summary>
/// A sellable product. Belongs to exactly one <see cref="ProductGroup"/> and has at
/// most one <see cref="ProductInfo"/> with the descriptive details.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ProductGroupId { get; set; }

    public ProductGroup? ProductGroup { get; set; }

    public ProductInfo? Info { get; set; }

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
