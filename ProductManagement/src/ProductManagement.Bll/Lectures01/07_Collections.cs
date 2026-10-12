namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>One row of a stock export.</summary>
public sealed record StockRow(string Sku, int Quantity);

// =====================================================================================

/// <summary>
/// STEP 7 — Collections: array, List, Dictionary, HashSet.
/// </summary>
/// <remarks>
/// Suggested time: 40-50 minutes.
///
/// The point: <b>pick the collection that matches the question you ask of it.</b> Most
/// slow ported code is a list being scanned in a loop where a dictionary would answer
/// instantly.
///
/// <list type="bullet">
/// <item><c>T[]</c> - fixed size, fastest.</item>
/// <item><c>List&lt;T&gt;</c> - the default. Ordered, grows, indexable; lookup by value
///       is a linear scan.</item>
/// <item><c>Dictionary&lt;TKey,TValue&gt;</c> - lookup by key in roughly constant time.
///       The COMPARER is part of the design.</item>
/// <item><c>HashSet&lt;T&gt;</c> - membership, no duplicates.</item>
/// </list>
///
/// Harbour has arrays and hashes:
/// <code>
///     aRows := { { "VEH-001", 3 }, { "TYR-001", 64 } }
///     nPos  := AScan( aRows, {| a | a[ 1 ] == cSku } )   // linear scan
///     hStock := hb_Hash()
///     hStock[ "VEH-001" ] := 3                           // constant-time lookup
/// </code>
/// AScan inside a loop is the exact shape of the performance bug in task 2.
/// </remarks>
public static class Step07_Collections
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - Build a quantity lookup
    //
    //   - SKU comparison is case-insensitive (the warehouse exports lowercase)
    //   - the same SKU may appear twice: sum the quantities
    // ---------------------------------------------------------------------------------

    /// <summary>Totals the quantities per SKU.</summary>
    public static Dictionary<string, int> TotalsBySku(IEnumerable<StockRow> rows)
    {
        // TODO 1: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        // TODO 2: foreach over the rows
        // TODO 3: TryGetValue for the current total (0 when the key is missing)
        // TODO 4: write back current + row.Quantity with the indexer
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static Dictionary<string, int> TotalsBySku_Solution(IEnumerable<StockRow> rows)
    {
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            totals.TryGetValue(row.Sku, out var current);   // 0 when missing
            totals[row.Sku] = current + row.Quantity;
        }

        return totals;
    }

    // - The default comparer is case-SENSITIVE, so "veh-001" and "VEH-001" would be two
    //   keys and the totals silently wrong.
    // - `dict[key] = value` overwrites; `dict.Add(...)` throws on a duplicate. Which you
    //   want is a business decision.
    // - ContainsKey followed by the indexer does the lookup twice; TryGetValue does one.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - Find what is missing
    //
    // Return the SKUs in `incoming` but not in `existing`, without duplicates, keeping the
    // order of `incoming`. Then: what would the List-based version cost with 50 000 each?
    // ---------------------------------------------------------------------------------

    /// <summary>SKUs present in incoming but not in existing.</summary>
    public static List<string> NewSkus(IEnumerable<string> existing, IEnumerable<string> incoming)
    {
        // TODO 1: a HashSet<string> from `existing`, case-insensitive
        // TODO 2: a List for the result
        // TODO 3: foreach over `incoming` - look at what HashSet.Add RETURNS
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static List<string> NewSkus_Solution(IEnumerable<string> existing, IEnumerable<string> incoming)
    {
        var known = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var sku in incoming)
        {
            // Add returns false if it was already there, so this one call filters out the
            // known ones AND suppresses duplicates within `incoming`.
            if (known.Add(sku))
            {
                result.Add(sku);
            }
        }

        return result;
    }

    /// <summary>The slow version, for the stopwatch comparison.</summary>
    public static List<string> NewSkus_Naive(IReadOnlyList<string> existing, IEnumerable<string> incoming)
    {
        var result = new List<string>();

        foreach (var sku in incoming)
        {
            if (!existing.Contains(sku))   // a full scan of `existing`, for EVERY incoming sku
            {
                result.Add(sku);
            }
        }

        return result;
    }

    // 50 000 x 50 000 is 2.5 billion comparisons; the HashSet version is 100 000. This
    // shape - "look this up in that list, inside a loop" - is the most common performance
    // bug in ported code, and it never shows up on the 20-row test file.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - Group by hand
    //
    // Group the SKUs by group code (the part before the dash), keeping input order.
    // A SKU with no dash is its own group.
    // ---------------------------------------------------------------------------------

    /// <summary>Group code -> the SKUs in that group.</summary>
    public static Dictionary<string, List<string>> GroupSkus(IEnumerable<StockRow> rows)
    {
        // TODO 1: the dictionary, case-insensitive
        // TODO 2: the group code: IndexOf('-') and the range sku[..i]
        // TODO 3: the add-to-bucket pattern:
        //         if (!dict.TryGetValue(key, out var list)) { list = new(); dict[key] = list; }
        // TODO 4: list.Add(sku)
        throw new NotImplementedException("Task 3");
    }

    #region Solution 3

    public static Dictionary<string, List<string>> GroupSkus_Solution(IEnumerable<StockRow> rows)
    {
        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var sku = row.Sku.Trim().ToUpperInvariant();
            var dashIndex = sku.IndexOf('-');
            var groupCode = dashIndex > 0 ? sku[..dashIndex] : sku;

            if (!groups.TryGetValue(groupCode, out var list))
            {
                list = new List<string>();
                groups[groupCode] = list;
            }

            list.Add(sku);
        }

        return groups;
    }

    // - `sku[..dashIndex]` is a range expression - Substring(0, n) with nicer syntax.
    // - `list` is a REFERENCE into the dictionary, so adding to it afterwards works.
    // - With LINQ this becomes `rows.GroupBy(r => GroupCodeOf(r.Sku))`. Doing it by hand
    //   first is what makes GroupBy click when we get there.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 4 - Which collection? (discussion, 10 minutes, no code)
    //   a) invoice lines, in entry order        d) the fixed list of VAT rates
    //   b) "already warned?", a million times   e) a work queue used by several threads
    //   c) a product cache keyed by id
    // ---------------------------------------------------------------------------------

    #region Solution 4

    // a) List<InvoiceLine>         b) HashSet<int>         c) Dictionary<int, Product>
    // d) a static readonly array   e) ConcurrentQueue<T>
    //
    // (e) is the one that bites: the plain Queue<T> is NOT thread-safe, and concurrent use
    // corrupts it SILENTLY rather than throwing.

    #endregion
}
