namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>One line of a basket. A record: immutable, value equality, `with` support.</summary>
public sealed record BasketLine(string Sku, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

// =====================================================================================

/// <summary>
/// STEP 5 — Classes, organisation and visibility.
/// </summary>
/// <remarks>
/// Suggested time: 50-60 minutes. This step pays off most for the rewrite.
///
/// <list type="bullet">
/// <item>namespace -> class -> member. A namespace is only naming; the real boundary is
///       the <b>assembly</b> (the project).</item>
/// <item>Defaults: a class is <c>internal</c>, a member is <c>private</c> - the opposite
///       of Harbour, where everything is global by default.</item>
/// <item><c>static</c> for pure calculations; instance when there is state or an injected
///       dependency.</item>
/// <item>A property is a method pair, not a field.</item>
/// </list>
///
/// The central parallel:
/// <code>
///     STATIC FUNCTION Normalize( cSku )   // visible only inside THIS .prg file
///     FUNCTION CalcGross( nNet )          // visible to the ENTIRE linked program
/// </code>
/// C# has a level in between: <c>internal</c> is "STATIC FUNCTION, but for a whole
/// project". See <c>dotnet-into.md</c> for the long version.
///
/// Lab closer: try to use an <c>internal</c> type of the Dal project from Bll and read
/// error CS0122.
/// </remarks>
public static class Step05_ClassesAndVisibility
{
    // ---------------------------------------------------------------------------------
    // TASK 3 (discussion, 10 minutes, no code) - static or instance, and why?
    //   a) converting a net price to gross
    //   b) a shopping basket that collects lines
    //   c) a service that reads products from the database
    //   d) a counter of orders handled since start-up
    // ---------------------------------------------------------------------------------

    #region Solution 3

    // a) static   - pure calculation
    // b) instance - it IS state
    // c) instance - it has a dependency (the DbContext) handed to it by DI
    // d) trick question. `static int _count` works until you want a second counter, a test
    //    that starts from zero, or two tenants in one process - and it needs
    //    Interlocked.Increment, not ++. Static FUNCTIONS are fine; static DATA is the trap.
    //    This is the same trap as a Harbour PUBLIC or file-level STATIC variable.

    #endregion
}

// =====================================================================================
// TASK 1 - A basket that cannot be corrupted
//
//   - lines can only be created through Add(); the list is unreachable from outside
//   - Add() rejects a blank sku, quantity <= 0 and a negative price
//   - adding a SKU that is already there MERGES: quantities summed, no second line
//   - SKU comparison is case-insensitive, stored uppercased
//   - Total and LineCount are COMPUTED, never stored
//
// Before writing it: list the public members on the whiteboard. That list is the contract.
// =====================================================================================

/// <summary>A shopping basket. Task 1.</summary>
public sealed class ShoppingBasket
{
    // TODO 1: a private readonly List<BasketLine> backing field.
    //         Discuss what `readonly` does and does NOT protect.

    /// <summary>The lines, read-only for the caller.</summary>
    public IReadOnlyList<BasketLine> Lines => throw new NotImplementedException("Task 1");

    /// <summary>Number of distinct lines.</summary>
    public int LineCount => throw new NotImplementedException("Task 1");

    /// <summary>Sum of the line totals.</summary>
    public decimal Total => throw new NotImplementedException("Task 1");

    /// <summary>Adds a line, or merges into the existing line for the same SKU.</summary>
    public void Add(string sku, int quantity, decimal unitPrice)
    {
        // TODO 2: validate the three arguments, using nameof() in every exception
        // TODO 3: normalise the sku (trim + uppercase) in ONE private helper
        // TODO 4: find an existing line with FindIndex
        // TODO 5: found -> replace with `existing with { Quantity = ... }`; else Add a new one
        throw new NotImplementedException("Task 1");
    }

    /// <summary>Removes the line for the SKU. False when there was nothing to remove.</summary>
    public bool Remove(string sku) => throw new NotImplementedException("Task 1");
}

#region Solution 1

/// <summary>Reference solution for Task 1.</summary>
public sealed class ShoppingBasket_Solution
{
    // private: unreachable from outside
    // readonly: the FIELD cannot be repointed; the CONTENT can still change
    private readonly List<BasketLine> _lines = new();

    // AsReadOnly returns a wrapper, so `basket.Lines as List<BasketLine>` is null.
    public IReadOnlyList<BasketLine> Lines => _lines.AsReadOnly();

    public int LineCount => _lines.Count;

    // Computed on every read: Total can never disagree with Lines.
    public decimal Total => _lines.Sum(line => line.LineTotal);

    public void Add(string sku, int quantity, decimal unitPrice)
    {
        var key = NormalizeSku(sku);

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive.");
        }

        if (unitPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), unitPrice, "Unit price cannot be negative.");
        }

        var index = _lines.FindIndex(line => line.Sku == key);

        if (index >= 0)
        {
            var existing = _lines[index];
            _lines[index] = existing with { Quantity = existing.Quantity + quantity, UnitPrice = unitPrice };
            return;
        }

        _lines.Add(new BasketLine(key, quantity, unitPrice));
    }

    public bool Remove(string sku)
    {
        var index = _lines.FindIndex(line => line.Sku == NormalizeSku(sku));

        if (index < 0)
        {
            return false;
        }

        _lines.RemoveAt(index);
        return true;
    }

    // private AND static: depends on nothing but its argument.
    private static string NormalizeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Sku is required.", nameof(sku));
        }

        return sku.Trim().ToUpperInvariant();
    }
}

// - The PUBLIC surface is the contract: Lines, LineCount, Total, Add, Remove. If _lines
//   were public, "store them in a Dictionary instead" would be a breaking change.
// - Because Add is the only way in, every line is valid by construction. There is no
//   "validate the basket" step anywhere, and there never needs to be.
// - `readonly` is about the VARIABLE, not the object: `_lines.Add()` still works.
// - Normalisation lives in ONE place. In Harbour this is typically UPPER(ALLTRIM(...))
//   repeated in forty procedures, and the forty-first forgets it.

#endregion

// =====================================================================================
// TASK 2 - Fix the visibility
//
// Everything below is public, the way a legacy port usually arrives. Rewrite it so:
//   - the VAT rate is supplied once in the constructor, validated there, then read-only
//   - rounding is an implementation detail
//   - ConvertInternalUnit is invisible from outside
//   - CalculateGross is the only public method
//
// Then call the private helper from outside and read the compiler error.
// =====================================================================================

/// <summary>Task 2 - fix the visibility of every member.</summary>
public sealed class PriceCalculator
{
    public const decimal DefaultVatPercent = 27m;

    public decimal VatPercent { get; set; } = DefaultVatPercent;

    public decimal CalculateGross(decimal net) => Round(net * (1 + VatPercent / 100m));

    public decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public decimal ConvertInternalUnit(decimal value) => value / 100m;
}

#region Solution 2

/// <summary>Reference solution for Task 2.</summary>
public sealed class PriceCalculator_Solution
{
    // public: part of the contract
    public const decimal DefaultVatPercent = 27m;

    // get-only: set in the constructor, immutable afterwards
    public decimal VatPercent { get; }

    public PriceCalculator_Solution(decimal vatPercent = DefaultVatPercent)
    {
        if (vatPercent is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(vatPercent), vatPercent, "VAT must be 0..100.");
        }

        VatPercent = vatPercent;
    }

    // public: the single reason this class exists
    public decimal CalculateGross(decimal net) => Round(net * (1 + VatPercent / 100m));

    // private: public would freeze the rounding rule into the contract for ever
    private static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    // private: nobody outside should know the storage format
    private static decimal ConvertInternalUnit(decimal value) => value / 100m;
}

// Every `public` is a promise. You can always ADD to a public surface later; you cannot
// take anything away without breaking somebody. Start narrow and widen on demand.
//
// This is the Harbour problem in reverse. There, FUNCTION means "the entire program may
// call this", so after fifteen years nobody can tell who does. Here the compiler tells
// you exactly who can reach a member - but only if you used something narrower than public.
//
// Moving Round to `internal` makes it visible to every type in ProductManagement.Bll and
// nobody else - not Web, not a test project (without InternalsVisibleTo).

#endregion
