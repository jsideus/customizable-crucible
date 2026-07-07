using Microsoft.Extensions.Configuration;

namespace Crucible.UI.Tests.Configuration;

public static class TestSettings
{
    private static readonly Lazy<QaCloudSettings> _qaCloud = new(Build);

    public static QaCloudSettings QaCloud => _qaCloud.Value;

    private static QaCloudSettings Build()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)   // forces a missing file to fail loudly and not silently bind null
            .AddUserSecrets<QaCloudSettings>()      //Local -> reads secrets.json
            .AddEnvironmentVariables()              //CI    -> local overrides if tests not run in CI
            .Build();

            return config.GetSection("QaCloud").Get<QaCloudSettings>()
                ?? throw new InvalidOperationException(
                    "QaCloud settings missing. Run: dotnet user-secrets set \"QaCloud:ApiToken\"...\"");
    }
}