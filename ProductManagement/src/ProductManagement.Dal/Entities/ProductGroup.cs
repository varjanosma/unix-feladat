namespace ProductManagement.Dal.Entities;

/// <summary>
/// A category products belong to. One group has many products.
/// </summary>
public class ProductGroup
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
