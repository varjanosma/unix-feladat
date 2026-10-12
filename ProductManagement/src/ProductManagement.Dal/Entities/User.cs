namespace ProductManagement.Dal.Entities;

/// <summary>
/// A customer of the system. One user has many purchases.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; }

    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
