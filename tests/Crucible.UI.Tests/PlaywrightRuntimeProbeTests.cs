using System.Runtime.CompilerServices;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Crucible.UI.Tests;

public class PlaywrightRuntimeProbeOne : PageTest
{
    [Fact]
    public Task Probe_Playwright_Instance_One()
    {
        ParallelProofLogger.Log(
            $"PLAYWRIGHT_PROBE {nameof(PlaywrightRuntimeProbeOne)} " +
            $"PlaywrightHash={RuntimeHelpers.GetHashCode(Playwright)} " +
            $"PID={Environment.ProcessId} " +
            $"Thread={Environment.CurrentManagedThreadId}");

            return Task.CompletedTask;
    }
}

public class PlaywrightRuntimeProbeTwo : PageTest
{
    [Fact]
    public Task Probe_Playwright_Instance_Two()
    {
        ParallelProofLogger.Log(
            $"PLAYWRIGHT_PROBE {nameof(PlaywrightRuntimeProbeTwo)} " +
            $"PlaywrightHash={RuntimeHelpers.GetHashCode(Playwright)} " +
            $"PID={Environment.ProcessId} " +
            $"Thread={Environment.CurrentManagedThreadId}");

            return Task.CompletedTask;
    }
}