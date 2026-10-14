namespace ProductManagement.Transfer;

/// <summary>Filter, sort and paging parameters of the product search. Bound from the query string.</summary>
public sealed record ProductSearchRequest
{
    public int? ProductGroupId { get; init; }

    public string? Search { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public bool ActiveOnly { get; init; } = true;

    public ProductSort Sort { get; init; } = ProductSort.Name;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
