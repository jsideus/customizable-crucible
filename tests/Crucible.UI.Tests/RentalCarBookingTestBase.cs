using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using Crucible.UI.Tests.Auth;

namespace Crucible.UI.Tests;

public class RentalCarBookingTestBase : PageTest
{
    public override async ValueTask InitializeAsync()
    {
        await RentalCarBookingAppAuth.EnsureAsync();        //where the login is done once before context is built
        await base.InitializeAsync();                       //ContextTest now successfully builds context from ContextOptions()
    }

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        StorageStatePath = RentalCarBookingAppAuth.StorageStatePath,
        Locale = "en-US",
        ColorScheme = ColorScheme.Light,
    };
}