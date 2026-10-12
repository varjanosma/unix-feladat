using System.Globalization;
using System.Text;

namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>A raw line from the supplier file. Everything is a string.</summary>
public sealed record RawRow(string? Sku, string? Name, string? Price);

/// <summary>The cleaned result of one row: either valid, or an error message.</summary>
public sealed record CleanRow(bool IsValid, string Sku, string Name, decimal Price, string Error)
{
    public static CleanRow Ok(string sku, string name, decimal price) => new(true, sku, name, price, string.Empty);

    public static CleanRow Fail(string error) => new(false, string.Empty, string.Empty, 0m, error);
}

/// <summary>Current state of a product on the edit screen.</summary>
public sealed record ProductState(string Name, decimal Price, int Quantity);

/// <summary>Only the fields the user touched. Null means "not sent".</summary>
public sealed record ProductPatch(string? Name, decimal? Price, int? Quantity);

// =====================================================================================

/// <summary>
/// STEP 2 — The type system: value vs reference, var, nullable, strings.
/// </summary>
/// <remarks>
/// Suggested time: 45-55 minutes.
///
/// <list type="bullet">
/// <item>A struct (int, decimal, DateTime, bool) is copied on assignment; a class is not.</item>
/// <item><c>var</c> is not "dynamic" - the compiler knows the exact type.</item>
/// <item><c>string</c> means "never null", <c>string?</c> means "can be null".</item>
/// <item>string is immutable: every "change" allocates a new one.</item>
/// </list>
///
/// Harbour has no declared types at all:
/// <code>
///     LOCAL xValue := "text"
///     xValue := 42            // perfectly legal
/// </code>
/// Here the type is fixed at compile time, which is why the editor can tell you what is
/// available on a value and why a typo is a build error rather than a support call.
/// </remarks>
public static class Step02_TypeSystem
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - Clean up an import row
    //
    //   Sku:   trimmed, uppercase. Missing or blank -> invalid.
    //   Name:  trimmed. Missing or blank -> invalid.
    //   Price: parsed with InvariantCulture. Not a number, or negative -> invalid.
    //
    // Never throw, never return null.
    //
    // Harbour would be:
    //     cSku := Upper( AllTrim( aRow[ 1 ] ) )
    //     nPrice := Val( aRow[ 3 ] )      // and garbage silently becomes 0
    // ---------------------------------------------------------------------------------

    /// <summary>Cleans one import row.</summary>
    public static CleanRow Clean(RawRow raw)
    {
        // TODO 1: trim the sku; null or whitespace -> CleanRow.Fail(...)
        // TODO 2: uppercase it with ToUpperInvariant (not ToUpper - ask why)
        // TODO 3: the name: same blank check, keep the original casing
        // TODO 4: decimal.TryParse with CultureInfo.InvariantCulture, then check >= 0
        // TODO 5: return CleanRow.Ok(...)
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static CleanRow Clean_Solution(RawRow raw)
    {
        var sku = raw.Sku?.Trim();
        if (string.IsNullOrWhiteSpace(sku))
        {
            return CleanRow.Fail("Sku is required.");
        }

        sku = sku.ToUpperInvariant();

        var name = raw.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return CleanRow.Fail($"{sku}: Name is required.");
        }

        if (!decimal.TryParse(raw.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            return CleanRow.Fail($"{sku}: '{raw.Price}' is not a valid price.");
        }

        if (price < 0m)
        {
            return CleanRow.Fail($"{sku}: the price cannot be negative.");
        }

        return CleanRow.Ok(sku, name, price);
    }

    // - ToUpper uses the CURRENT culture; in Turkish "i" becomes "İ" and the SKU stops
    //   matching. For identifiers always ToUpperInvariant. Same for parsing: "18900.50"
    //   read with the Hungarian culture becomes 1890050.
    // - TryParse instead of Val(): garbage is reported, not silently turned into 0.
    // - `raw.Sku?.Trim()` - if Sku is null the whole expression is null, no exception.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - Build a price list
    //
    //     "VEH-001     Volkswagen Golf                 18900.00"
    //      SKU to 12    name to 30                      price right-aligned in 10, F2
    //
    // Skip invalid rows. Use a StringBuilder, then write the `result += ...` version and
    // time both over 100 000 rows.
    // ---------------------------------------------------------------------------------

    /// <summary>Renders one line per valid row.</summary>
    public static string RenderList(IEnumerable<CleanRow> rows)
    {
        // TODO 1: create a StringBuilder
        // TODO 2: foreach, skipping invalid rows with `continue`
        // TODO 3: Append sku.PadRight(12), name.PadRight(30) and
        //         price.ToString("F2", CultureInfo.InvariantCulture).PadLeft(10)
        // TODO 4: AppendLine, then return builder.ToString()
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static string RenderList_Solution(IEnumerable<CleanRow> rows)
    {
        var builder = new StringBuilder();

        foreach (var row in rows)
        {
            if (!row.IsValid)
            {
                continue;
            }

            builder
                .Append(row.Sku.PadRight(12))
                .Append(row.Name.PadRight(30))
                .Append(row.Price.ToString("F2", CultureInfo.InvariantCulture).PadLeft(10))
                .AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>The slow version, for the comparison.</summary>
    public static string RenderList_Naive(IEnumerable<CleanRow> rows)
    {
        var result = string.Empty;

        foreach (var row in rows)
        {
            // Every += allocates a new string and copies everything written so far.
            result += row.Sku + " " + row.Name + Environment.NewLine;
        }

        return result;
    }

    // The StringBuilder rule is about LOOPS. Three concatenations in a row are fine with `+`.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - Apply a partial patch
    //
    // The edit screen sends only the fields the user touched.
    //     null  -> leave unchanged
    //     ""    -> for Name only: cleared, becomes "(no name)"
    //     value -> use it (trimmed, for Name)
    //
    // Do NOT modify `current` - return a new ProductState.
    //
    // Before writing code: what is the type of `var x = patch.Price;`?
    // And of `var y = patch.Price ?? current.Price;`?
    // ---------------------------------------------------------------------------------

    /// <summary>Applies a patch without mutating the original state.</summary>
    public static ProductState ApplyPatch(ProductState current, ProductPatch patch)
    {
        // TODO 1: the name - three cases, not two. A switch expression reads well.
        // TODO 2: price - `patch.Price ?? current.Price`
        // TODO 3: quantity - the same; note 0 is a real value, not "missing"
        // TODO 4: return `current with { ... }`, not an assignment to current
        throw new NotImplementedException("Task 3");
    }

    #region Solution 3

    public static ProductState ApplyPatch_Solution(ProductState current, ProductPatch patch)
    {
        var name = patch.Name switch
        {
            null => current.Name,       // not sent
            "" => "(no name)",          // sent, empty -> cleared
            _ => patch.Name.Trim(),
        };

        return current with
        {
            Name = name,
            Price = patch.Price ?? current.Price,
            Quantity = patch.Quantity ?? current.Quantity,
        };
    }

    // - `var x = patch.Price;` is decimal?; `patch.Price ?? current.Price` is decimal.
    //   Hover over `var` and the IDE tells you - that is the point of it being compile-time.
    // - Three-way (null / empty / value) is what catches people. In Harbour NIL and ""
    //   are usually treated the same; here they are different states, and conflating them
    //   silently loses data.
    // - `with` makes a copy. A mutable class assigned in place would change the object
    //   the caller still holds.

    #endregion
}
