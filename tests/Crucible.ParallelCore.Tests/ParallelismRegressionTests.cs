namespace Crucible.ParallelCore.Tests;

//<summary>
/// Parallelism regression probe. Total run time should be under ~700ms;
/// if it climbs back toward ~1.5s, intra-collection parallelism has regressed.
/// Each probe lives in its own class because xUnit parallelizes by collection
/// (one class = one collection), not within a class.
//</summary>

public class ParallelProbeATest
{
    private readonly ITestOutputHelper _output;
    public ParallelProbeATest(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Probe()
    {
        var start = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_A START thread={Environment.CurrentManagedThreadId} at {start:HH:mm:ss.fff}");
        await Task.Delay(500, TestContext.Current.CancellationToken);
        var end = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_A END thread={Environment.CurrentManagedThreadId} at {end:HH:mm:ss.fff}");
    }
}

public class ParallelProbeBTest
{
    private readonly ITestOutputHelper _output;
    public ParallelProbeBTest(ITestOutputHelper output) => _output = output;
    [Fact]
    public async Task Probe()
    {
        var start = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_B START thread={Environment.CurrentManagedThreadId} at {start:HH:mm:ss.fff}");
        await Task.Delay(500, TestContext.Current.CancellationToken);
        var end = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_B END thread={Environment.CurrentManagedThreadId} at {end:HH:mm:ss.fff}");
    }
}

public class ParallelProbeCTest
{
    private readonly ITestOutputHelper _output;
    public ParallelProbeCTest(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Probe()
    {
        var start = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_C START thread={Environment.CurrentManagedThreadId} at {start:HH:mm:ss.fff}");
        await Task.Delay(500, TestContext.Current.CancellationToken);
        var end = DateTimeOffset.UtcNow;
        _output.WriteLine($"Probe_C END thread={Environment.CurrentManagedThreadId} at {end:HH:mm:ss.fff}");
    }
}