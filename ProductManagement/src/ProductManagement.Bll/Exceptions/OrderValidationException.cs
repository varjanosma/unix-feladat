namespace ProductManagement.Bll.Exceptions;

/// <summary>The order request itself is wrong. Carries every problem, not just the first one.</summary>
public sealed class OrderValidationException : Exception
{
    public OrderValidationException(IReadOnlyList<string> errors)
        : base("The order is invalid: " + string.Join(" ", errors))
        => Errors = errors;

    public IReadOnlyList<string> Errors { get; }
}
