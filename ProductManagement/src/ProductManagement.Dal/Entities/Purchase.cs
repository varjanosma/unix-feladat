namespace ProductManagement.Dal.Entities;

/// <summary>
/// One purchase of a user (the header). The purchased products are the
/// <see cref="PurchaseItem"/> lines.
/// </summary>
public class Purchase
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public DateTime PurchasedAtUtc { get; set; }

    public string? Note { get; set; }

    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}
