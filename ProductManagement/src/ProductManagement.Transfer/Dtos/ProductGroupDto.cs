namespace ProductManagement.Transfer;

/// <summary>
/// A product group as the client sees it. DTOs are deliberately separate from the
/// entities: the API contract must not change just because a column was renamed.
/// </summary>
public sealed record ProductGroupDto
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int ProductCount { get; init; }
}
