using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 3 — if, switch, patterns, loops.</summary>
public class Step03_ControlFlowTests
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static decimal ShippingFee(ShippingContext context) => Step03_ControlFlow.ShippingFee_Solution(context);

    private static (Dictionary<string, int> Closing, List<string> Errors) Replay(
        IReadOnlyDictionary<string, int> opening,
        IEnumerable<StockMovement> movements)
        => Step03_ControlFlow.Replay_Solution(opening, movements);
    // ---------------------------------------------------------------------------------

    private static ShippingContext Order(
        bool vehicle = false,
        bool express = false,
        decimal total = 10_000m,
        decimal weight = 1m)
        => new(vehicle, express, total, weight);

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void VehicleBeatsEveryOtherRule()
        => Assert.Equal(0m, ShippingFee(Order(vehicle: true, express: true, weight: 500m)));

    [Fact]
    public void ExpressBeatsWeightAndTotal()
        => Assert.Equal(4_900m, ShippingFee(Order(express: true, weight: 100m, total: 90_000m)));

    [Fact]
    public void HeavyOrderPaysTheHeavyFee()
        => Assert.Equal(3_900m, ShippingFee(Order(weight: 30.5m)));

    [Fact]
    public void ExactlyThirtyKilosIsNotHeavy()
        => Assert.Equal(1_490m, ShippingFee(Order(weight: 30m)));

    [Fact]
    public void BigOrdersShipFree()
        => Assert.Equal(0m, ShippingFee(Order(total: 50_000m)));

    [Fact]
    public void EverythingElsePaysTheFlatFee()
        => Assert.Equal(1_490m, ShippingFee(Order()));

    [Fact]
    public void TheSwitchVersionAgreesWithTheIfVersion()
    {
        ShippingContext[] cases =
        {
            Order(vehicle: true),
            Order(express: true),
            Order(express: true, total: 200_000m),
            Order(weight: 45m),
            Order(total: 60_000m),
            Order(),
        };

        foreach (var context in cases)
        {
            Assert.Equal(
                Step03_ControlFlow.ShippingFee_Solution(context),
                Step03_ControlFlow.ShippingFee_SolutionSwitch(context));
        }
    }

    // --- TASK 2 ----------------------------------------------------------------------

    private static readonly Dictionary<string, int> Opening =
        new(StringComparer.OrdinalIgnoreCase) { ["VEH-001"] = 3, ["TYR-001"] = 64 };

    [Fact]
    public void ReceiptsAndIssuesAreApplied()
    {
        var movements = new[]
        {
            new StockMovement(1, "VEH-001", MovementKind.Receipt, 2),
            new StockMovement(2, "TYR-001", MovementKind.Issue, 4),
        };

        var (closing, errors) = Replay(Opening, movements);

        Assert.Equal(5, closing["VEH-001"]);
        Assert.Equal(60, closing["TYR-001"]);
        Assert.Empty(errors);
    }

    [Fact]
    public void CancelledMovementsAreSkippedSilently()
    {
        var movements = new[] { new StockMovement(1, "VEH-001", MovementKind.Cancelled, 99) };

        var (closing, errors) = Replay(Opening, movements);

        Assert.Equal(3, closing["VEH-001"]);
        Assert.Empty(errors);
    }

    [Fact]
    public void AnUnknownSkuIsRecordedAndTheBatchContinues()
    {
        var movements = new[]
        {
            new StockMovement(1, "NOPE-001", MovementKind.Receipt, 5),
            new StockMovement(2, "VEH-001", MovementKind.Receipt, 1),
        };

        var (closing, errors) = Replay(Opening, movements);

        Assert.Single(errors);
        Assert.Contains("NOPE-001", errors[0], StringComparison.Ordinal);
        Assert.Equal(4, closing["VEH-001"]);     // the next row was still applied
    }

    [Fact]
    public void GoingBelowZeroIsRejectedAndTheQuantityIsKept()
    {
        var movements = new[] { new StockMovement(7, "VEH-001", MovementKind.Issue, 10) };

        var (closing, errors) = Replay(Opening, movements);

        Assert.Single(errors);
        Assert.Equal(3, closing["VEH-001"]);
    }

    [Fact]
    public void SkuMatchingIsCaseInsensitive()
    {
        var movements = new[] { new StockMovement(1, "veh-001", MovementKind.Receipt, 1) };

        var (closing, errors) = Replay(Opening, movements);

        Assert.Empty(errors);
        Assert.Equal(4, closing["VEH-001"]);
    }

    [Fact]
    public void ReplayDoesNotModifyTheOpeningQuantities()
    {
        var opening = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["VEH-001"] = 3 };

        Replay(opening, new[] { new StockMovement(1, "VEH-001", MovementKind.Receipt, 10) });

        Assert.Equal(3, opening["VEH-001"]);
    }

    // --- TASK 3: the three loop bugs -------------------------------------------------

    [Fact]
    public void TheOriginalRemoveCancelledThrows()
    {
        var movements = new List<StockMovement>
        {
            new(1, "A-001", MovementKind.Cancelled, 1),
            new(2, "A-002", MovementKind.Receipt, 1),
        };

        Assert.Throws<InvalidOperationException>(() => Step03_ControlFlow.RemoveCancelled(movements));
    }

    [Fact]
    public void BothFixesRemoveEveryCancelledRow()
    {
        static List<StockMovement> Sample() => new()
        {
            new(1, "A-001", MovementKind.Cancelled, 1),
            new(2, "A-002", MovementKind.Cancelled, 1),
            new(3, "A-003", MovementKind.Receipt, 1),
        };

        var byIndex = Sample();
        Step03_ControlFlow.RemoveCancelled_SolutionA(byIndex);

        var byRemoveAll = Sample();
        Step03_ControlFlow.RemoveCancelled_SolutionB(byRemoveAll);

        Assert.Single(byIndex);
        Assert.Single(byRemoveAll);
        Assert.Equal(3, byIndex[0].Id);
    }

    [Fact]
    public void SumFirstCountsExactlyTheRequestedRows()
    {
        var movements = new List<StockMovement>
        {
            new(1, "A-001", MovementKind.Receipt, 10),
            new(2, "A-002", MovementKind.Receipt, 20),
            new(3, "A-003", MovementKind.Receipt, 30),
        };

        Assert.Equal(30, Step03_ControlFlow.SumFirst_Solution(movements, 2));
        Assert.Equal(0, Step03_ControlFlow.SumFirst_Solution(movements, 0));
        Assert.Equal(60, Step03_ControlFlow.SumFirst_Solution(movements, 99));   // clamped
    }

    [Fact]
    public void TheOriginalSumFirstReadsOneRowTooMany()
    {
        var movements = new List<StockMovement> { new(1, "A-001", MovementKind.Receipt, 10) };

        Assert.Throws<ArgumentOutOfRangeException>(() => Step03_ControlFlow.SumFirst(movements, 1));
    }

    [Fact]
    public void FindFirstReturnsTheFirstMatchNotTheLast()
    {
        var movements = new[]
        {
            new StockMovement(1, "A-001", MovementKind.Receipt, 100),
            new StockMovement(2, "A-002", MovementKind.Issue, 50),
            new StockMovement(3, "A-003", MovementKind.Issue, 70),
        };

        Assert.Equal(2, Step03_ControlFlow.FindFirstLargeIssue_Solution(movements, 10)!.Id);
        Assert.Equal(3, Step03_ControlFlow.FindFirstLargeIssue(movements, 10)!.Id);   // the bug
        Assert.Null(Step03_ControlFlow.FindFirstLargeIssue_Solution(movements, 1000));
    }
}
