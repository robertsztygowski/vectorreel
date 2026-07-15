using MdReel.Core.Domain;

namespace MdReel.Tests.Unit.Domain;

public sealed class CostLedgerTests
{
    [Fact]
    public void Records_entries_in_order()
    {
        var ledger = new InMemoryCostLedger();

        ledger.Record(new CostEntry("job-1", CostKind.Compute, "stage_a.probe", 0.2, "seconds"));
        ledger.Record(new CostEntry("job-1", CostKind.Compute, "stage_a.scan", 9.3, "seconds"));

        Assert.Collection(
            ledger.Entries,
            e => Assert.Equal("stage_a.probe", e.Step),
            e => Assert.Equal("stage_a.scan", e.Step));
    }

    // Stage A can count ffmpeg seconds honestly but cannot price them: the Cloud Run rate is not
    // knowable from a laptop, and a made-up rate in the ledger is worse than an admitted gap.
    [Fact]
    public void Compute_entries_carry_a_quantity_but_no_invented_price()
    {
        var entry = new CostEntry("job-1", CostKind.Compute, "stage_a.scan", 9.3, "seconds");

        Assert.Equal(9.3, entry.Quantity);
        Assert.Null(entry.Cents);
    }
}
