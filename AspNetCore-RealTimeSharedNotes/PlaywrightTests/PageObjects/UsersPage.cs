using Microsoft.Playwright;

namespace AspNetCore_RealTimeSharedNotes.PlaywrightTests.PageObjects;

public class UsersPage(IPage page)
{
    private readonly IPage _page = page;

    public async Task GoToAsync()
    {
        await _page.GotoAsync(Config.TestConfig.BaseUrl + "/users");
        //wait for the async user list to finish loading
        await _page.GetByTestId("users-loading").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden
        });
    }

    public async Task CreateUserAsync(string email, string password, string role)
    {
        await _page.GetByTestId("create-email-input").FillAsync(email);
        await _page.GetByTestId("create-password-input").FillAsync(password);
        await _page.GetByTestId("create-role-select").SelectOptionAsync(new SelectOptionValue { Label = role });
        await _page.GetByTestId("create-user-btn").ClickAsync();

        //api key modal only appears on successful user creation, waiting for it as confirmation
        await _page.GetByTestId("apikey-modal").WaitForAsync();
        await _page.GetByTestId("apikey-modal-close").ClickAsync();

        await _page.GetByTestId("users-loading").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden
        });
    }

    //check if user email exists in users list
    public async Task<bool> UserExistsAsync(string email)
    {
        var emails = await _page.GetByTestId("user-email").AllInnerTextsAsync();
        return emails.Any(e => e.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
