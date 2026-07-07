namespace Crucible.UI.Tests;

public static class ParallelProofLogger
{
    private static readonly object Lock = new();

    public static void Log(string message)
    {
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestResults",
            "parallel-proof.log");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var line = 
            $"{DateTimeOffset.UtcNow:O} | " +
            $"PID={Environment.ProcessId} | " +
            $"Thread={Environment.CurrentManagedThreadId} | " +
            $"{message}{Environment.NewLine}";

        lock (Lock)
        {
            File.AppendAllText(path, line);
        }
    }
}