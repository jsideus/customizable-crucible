using System.Text.RegularExpressions;
using Crucible.UI.Tests.Configuration;
using Crucible.UI.Tests.Pages;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Crucible.UI.Tests;

public class CloudDevPlatformTestsOne : PageTest
{
    private static void LogParallelProof(string message)
    {
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestResults",
            "parallel-proof.log");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        File.AppendAllText(
            path,
            $"{DateTimeOffset.UtcNow:O} | PID={Environment.ProcessId} | Thread={Environment.CurrentManagedThreadId} | {message}{Environment.NewLine}");
    }
    private readonly ITestOutputHelper _output;
    
    public CloudDevPlatformTestsOne(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task HasTitle()
    {
        _output.WriteLine(
           $"START {nameof(CloudDevPlatformTestsOne)}.{nameof(HasTitle)} " +
           $"Thread={Environment.CurrentManagedThreadId} " +
           $"Time={DateTimeOffset.UtcNow:O}");


        LogParallelProof($"START {nameof(CloudDevPlatformTestsOne)}.{nameof(HasTitle)}");

        await Page.GotoAsync("https://qacloud.dev");

        await Expect(Page).ToHaveTitleAsync(new Regex("QA CLOUD"));

        LogParallelProof($"END {nameof(CloudDevPlatformTestsOne)}.{nameof(HasTitle)}");

        _output.WriteLine(
            $"END {nameof(CloudDevPlatformTestsOne)}.{nameof(HasTitle)} " + 
            $"Thread={Environment.CurrentManagedThreadId} " +
            $"Time={DateTimeOffset.UtcNow:O}");
    }
}

public class CloudDevPlatformTestsTwo: PageTest
{
    private static void LogParallelProof(string message)
    {
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestResults",
            "parallel-proof.log");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        File.AppendAllText(
            path,
            $"{DateTimeOffset.UtcNow:O} | PID={Environment.ProcessId} | Thread={Environment.CurrentManagedThreadId} | {message}{Environment.NewLine}");
    }
    private readonly ITestOutputHelper _output;

    public CloudDevPlatformTestsTwo(ITestOutputHelper output)
    {
      _output = output;    
    }

    [Fact]
    public async Task GetApplicationsLink()
    {
        _output.WriteLine(
            $"START {nameof(CloudDevPlatformTestsTwo)}.{nameof(GetApplicationsLink)} " +
            $"Thread={Environment.CurrentManagedThreadId} " +
            $"Time={DateTimeOffset.UtcNow:O}");


        LogParallelProof($"START {nameof(CloudDevPlatformTestsTwo)}.{nameof(GetApplicationsLink)}");
        await Page.GotoAsync("https://www.qacloud.dev");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Applications"}).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Apps"})).ToBeVisibleAsync();

        LogParallelProof($"END {nameof(CloudDevPlatformTestsTwo)}.{nameof(GetApplicationsLink)}");

        _output.WriteLine(
            $"END {nameof(CloudDevPlatformTestsTwo)}.{nameof(GetApplicationsLink)} " +
            $"Thread={Environment.CurrentManagedThreadId} " +
            $"Time={DateTimeOffset.UtcNow:O}");
    }
}

public class RentalCarBookingTests : RentalCarBookingTestBase
{

    [Fact]
    public async Task SearchReturnsAvailableCarsForValidDateRange()
    {
// ARRANGE 
        string pickUpIsoDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
        string returnIsoDate = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");
        var rentalCarBookingSearchPage = new RentalSearchPage(Page);

        await Page.GotoAsync(TestSettings.QaCloud.RentalAppSearchCarsPage);

//ACT
    await rentalCarBookingSearchPage.SearchAsync(pickUpIsoDate, returnIsoDate);

// ASSERT
    await Expect(rentalCarBookingSearchPage.CarCount).ToHaveTextAsync(new Regex(@"\d+ cars found"));
    }
}

// ACT populate the target native form controls using stable DOM id markers
// await Page.GotoAsync(TestSettings.QaCloud.RentalAppLoginPage);

// await Page.GetByLabel("API Token").FillAsync(TestSettings.QaCloud.ApiToken);

// await Page.GetByRole(AriaRole.Button, new() { Name = "Open App"}).ClickAsync();

//         await Page.Locator("#searchStart").FillAsync(pickUpIsoDate);
//         await Page.Locator("#searchEnd").FillAsync(returnIsoDate);

//         var networkResponse = await Page.RunAndWaitForResponseAsync(async () =>    // Intercepts asynchronous AJAX traffic directly on the *browser network* transport layer
//             { 
//                 await Page.GetByRole(AriaRole.Button, new() { NameRegex =  new Regex("Search Cars", RegexOptions.IgnoreCase)}).ClickAsync();

//             }, response => 

//                  new Regex(@"\/cars\?").IsMatch(response.Url) &&                   // Matches the relative date path regardless of the URL date parameters
//                 (response.Status == 200 || response.Status == 304)
//         );

// //ASSERT
//         await Expect(Page.Locator("#carCount")).ToHaveTextAsync(new Regex(@"\d+ cars found"));      // Validate UI renders matching result nodes while using Regex for number of cars data variance

