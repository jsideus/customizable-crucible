using Microsoft.Playwright;
using Crucible.UI.Tests.Configuration;

namespace Crucible.UI.Tests.Auth;

public static class RentalCarBookingAppAuth
{
    public static readonly string StorageStatePath = 
        Path.Combine(AppContext.BaseDirectory, "auth", "rentalCarBookingApp.json");

    private static readonly Lazy<Task> _ensure = new(LoginAndSaveAsync);

    public static Task EnsureAsync() => _ensure.Value;

    private static async Task LoginAndSaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorageStatePath)!);

        var settings = TestSettings.QaCloud;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(settings.RentalAppLoginPage);
        await page.GetByLabel("API Token").FillAsync(settings.ApiToken);
        await page.GetByRole(AriaRole.Button, new() { Name = "Open App"}).ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Find Your Perfect Ride" })
                  .WaitForAsync();    //Confirms login landed on final destination domain before capturing state

        await context.StorageStateAsync(new() { Path = StorageStatePath });
    }
}