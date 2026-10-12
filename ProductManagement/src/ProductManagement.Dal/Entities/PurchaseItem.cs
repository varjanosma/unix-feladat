namespace ProductManagement.Dal.Entities;

/// <summary>
/// One line of a purchase: which product, how many, at what unit price.
/// The price is stored on the line, because the product price can change later.
/// </summary>
public class PurchaseItem
{
    public int Id { get; set; }

    public int PurchaseId { get; set; }

    public Purchase? Purchase { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
