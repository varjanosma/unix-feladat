namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>What a shipping fee depends on.</summary>
public sealed record ShippingContext(bool ContainsVehicle, bool Express, decimal OrderTotal, decimal WeightKg);

/// <summary>Kinds of stock movement.</summary>
public enum MovementKind
{
    Receipt = 1,
    Issue = 2,
    Cancelled = 3,
}

/// <summary>One stock movement row.</summary>
public sealed record StockMovement(int Id, string Sku, MovementKind Kind, int Quantity);

// =====================================================================================

/// <summary>
/// STEP 3 — Control flow: if, switch, switch expressions, loops.
/// </summary>
/// <remarks>
/// Suggested time: 40-50 minutes. The group knows what a loop is; the value is in the
/// C# specifics.
///
/// <list type="bullet">
/// <item><c>switch</c> has no fall-through: every branch ends with break/return/throw.</item>
/// <item>The switch <b>expression</b> returns a value and must be exhaustive.</item>
/// <item>Patterns: relational (<c>&lt; 0</c>), property (<c>{ Express: true }</c>),
///       combinators (<c>is &lt; 4 or &gt; 5</c>).</item>
/// <item>Modifying a collection while iterating it throws - and that is a feature.</item>
/// </list>
///
/// Harbour equivalents, for orientation:
/// <code>
///     DO CASE
///        CASE nQty &lt; 0 ; cStatus := "invalid"
///        CASE nQty == 0 ; cStatus := "out of stock"
///        OTHERWISE      ; cStatus := "available"
///     ENDCASE
///
///     FOR i := 1 TO Len( aRows ) ... NEXT
///     FOR EACH oRow IN aRows ... NEXT
/// </code>
/// </remarks>
public static class Step03_ControlFlow
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - Shipping fee. First match wins:
    //
    //   1. contains a vehicle        -> 0       (collected in person)
    //   2. express delivery          -> 4 900
    //   3. heavier than 30 kg        -> 3 900
    //   4. total at least 50 000     -> 0
    //   5. anything else             -> 1 490
    //
    // Write it with if/else, then as a switch expression, and compare the two.
    // ---------------------------------------------------------------------------------

    /// <summary>Shipping fee for an order.</summary>
    public static decimal ShippingFee(ShippingContext context)
    {
        // TODO 1: rule 1 - an early return keeps the happy path at the left edge
        // TODO 2: rules 2 and 3
        // TODO 3: rule 4
        // TODO 4: rule 5 - the default
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static decimal ShippingFee_Solution(ShippingContext context)
    {
        if (context.ContainsVehicle)
        {
            return 0m;
        }

        if (context.Express)
        {
            return 4_900m;
        }

        if (context.WeightKg > 30m)
        {
            return 3_900m;
        }

        if (context.OrderTotal >= 50_000m)
        {
            return 0m;
        }

        return 1_490m;
    }

    /// <summary>The same rules as a switch expression over the object itself.</summary>
    public static decimal ShippingFee_SolutionSwitch(ShippingContext context)
        => context switch
        {
            { ContainsVehicle: true } => 0m,
            { Express: true } => 4_900m,
            { WeightKg: > 30m } => 3_900m,
            { OrderTotal: >= 50_000m } => 0m,
            _ => 1_490m,
        };

    // - `{ WeightKg: > 30m }` is a property pattern with a relational pattern inside.
    // - RULE ORDER is the whole problem, and it is invisible in both versions unless you
    //   read top to bottom. Keep rule sets small, and write one test per rule.
    // - The switch version reads closer to the table the business wrote; the if version
    //   is easier to step through. Both are fine; a 200-line nested if is not.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - Replay stock movements
    //
    //   Receipt -> add, Issue -> subtract, Cancelled -> skip
    //   unknown SKU            -> record an error, carry on
    //   would go below zero    -> record an error, leave the quantity unchanged
    //   do NOT modify `opening` - return a new dictionary
    // ---------------------------------------------------------------------------------

    /// <summary>Closing quantities plus whatever went wrong.</summary>
    public static (Dictionary<string, int> Closing, List<string> Errors) Replay(
        IReadOnlyDictionary<string, int> opening,
        IEnumerable<StockMovement> movements)
    {
        // TODO 1: copy `opening` into a new Dictionary you may modify
        // TODO 2: create the error list
        // TODO 3: foreach - skip Cancelled with `continue`
        // TODO 4: TryGetValue; unknown sku -> error + `continue`
        // TODO 5: the new quantity, with a switch expression over movement.Kind
        // TODO 6: negative -> error + `continue`; otherwise write it back
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static (Dictionary<string, int> Closing, List<string> Errors) Replay_Solution(
        IReadOnlyDictionary<string, int> opening,
        IEnumerable<StockMovement> movements)
    {
        var quantities = new Dictionary<string, int>(opening, StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var movement in movements)
        {
            if (movement.Kind == MovementKind.Cancelled)
            {
                continue;
            }

            if (!quantities.TryGetValue(movement.Sku, out var current))
            {
                errors.Add($"Unknown SKU '{movement.Sku}' in movement {movement.Id}.");
                continue;
            }

            var updated = movement.Kind switch
            {
                MovementKind.Receipt => current + movement.Quantity,
                MovementKind.Issue => current - movement.Quantity,
                _ => throw new ArgumentOutOfRangeException(nameof(movements), movement.Kind, "Unhandled kind"),
            };

            if (updated < 0)
            {
                errors.Add($"Movement {movement.Id} would take {movement.Sku} to {updated}; kept at {current}.");
                continue;
            }

            quantities[movement.Sku] = updated;
        }

        return (quantities, errors);
    }

    // - `continue` as a guard is the loop version of an early return. The alternative is
    //   three levels of nesting, and the business rule disappears into the indentation.
    // - "Record and carry on" vs "throw" is a BUSINESS decision. A nightly batch that dies
    //   on row 7 000 of 10 000 is useless; an interactive save that half-applies is worse.
    // - The default branch THROWS. Add MovementKind.Transfer next year and the first test
    //   run tells you; a silent `_ => current` would lose stock for months.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - Find and fix the three loop bugs. Predict, run, then fix.
    // ---------------------------------------------------------------------------------

    /// <summary>Bug 1 - removes the cancelled movements.</summary>
    public static void RemoveCancelled(List<StockMovement> movements)
    {
        foreach (var movement in movements)
        {
            if (movement.Kind == MovementKind.Cancelled)
            {
                movements.Remove(movement);
            }
        }
    }

    /// <summary>Bug 2 - sums the first `count` quantities.</summary>
    public static int SumFirst(IReadOnlyList<StockMovement> movements, int count)
    {
        var total = 0;

        for (var i = 0; i <= count; i++)
        {
            total += movements[i].Quantity;
        }

        return total;
    }

    /// <summary>Bug 3 - finds the first issue of at least the given size.</summary>
    public static StockMovement? FindFirstLargeIssue(IEnumerable<StockMovement> movements, int minQuantity)
    {
        StockMovement? found = null;

        foreach (var movement in movements)
        {
            if (movement.Kind == MovementKind.Issue && movement.Quantity >= minQuantity)
            {
                found = movement;
            }
        }

        return found;
    }

    #region Solution 3

    // Bug 1 - InvalidOperationException: "Collection was modified". Two fixes:

    /// <summary>Backwards by index: removing at i does not disturb what is still ahead.</summary>
    public static void RemoveCancelled_SolutionA(List<StockMovement> movements)
    {
        for (var i = movements.Count - 1; i >= 0; i--)
        {
            if (movements[i].Kind == MovementKind.Cancelled)
            {
                movements.RemoveAt(i);
            }
        }
    }

    /// <summary>Or let the framework do it in one pass.</summary>
    public static void RemoveCancelled_SolutionB(List<StockMovement> movements)
        => movements.RemoveAll(m => m.Kind == MovementKind.Cancelled);

    // Say explicitly that the exception is a GOOD thing. In Harbour,
    //     FOR EACH oRow IN aRows ; ADel( aRows, oRow:__enumIndex() ) ; NEXT
    // silently skips every second match and you find out in production.

    /// <summary>Bug 2 - off by one: `i &lt;= count` reads one element too many.</summary>
    public static int SumFirst_Solution(IReadOnlyList<StockMovement> movements, int count)
    {
        var limit = Math.Clamp(count, 0, movements.Count);
        var total = 0;

        for (var i = 0; i < limit; i++)
        {
            total += movements[i].Quantity;
        }

        return total;
    }

    /// <summary>Bug 3 - it said "first" but returned the LAST match: the loop never stopped.</summary>
    public static StockMovement? FindFirstLargeIssue_Solution(IEnumerable<StockMovement> movements, int minQuantity)
    {
        foreach (var movement in movements)
        {
            if (movement.Kind == MovementKind.Issue && movement.Quantity >= minQuantity)
            {
                return movement;
            }
        }

        return null;
    }

    // Later, with LINQ, this whole method becomes one line:
    //     movements.FirstOrDefault(m => m.Kind == MovementKind.Issue && m.Quantity >= minQuantity)

    #endregion
}
