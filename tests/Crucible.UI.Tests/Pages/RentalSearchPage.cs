using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Crucible.UI.Tests.Pages;

public sealed class RentalSearchPage(IPage page)
{
    private ILocator PickUpDateInput => page.Locator("#searchStart");
    private ILocator ReturnDateInput => page.Locator("#searchEnd");
    private ILocator SearchCarsBtn => 
        page.GetByRole(AriaRole.Button, new() { NameRegex = new("Search Cars", RegexOptions.IgnoreCase) });

    public ILocator CarCount => page.Locator("#carCount");

    //Strict ISO format to comply with the expected format used by native inputs used in this app
    public async Task<IResponse> SearchAsync(string pickUpIsoDate, string returnIsoDate)
    {
        await PickUpDateInput.FillAsync(pickUpIsoDate);
        await ReturnDateInput.FillAsync(returnIsoDate);

        return await page.RunAndWaitForResponseAsync(
            () => SearchCarsBtn.ClickAsync(),
            response => Regex.IsMatch(response.Url, @"/cars\?") && response.Status is 200 or 304);
    }
}